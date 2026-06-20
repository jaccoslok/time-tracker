using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Models;
using TimeTracker.Repositories;

namespace TimeTracker.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ProjectRepository _projectRepository = new();
    private readonly TaskRepository _taskRepository = new();
    private readonly TimeEntryRepository _timeEntryRepository = new();

    public ObservableCollection<Project> Projects { get; } = [];
    public ObservableCollection<ProjectTask> Tasks { get; } = [];
    public ObservableCollection<TimeEntry> TimeEntries { get; } = [];

    // --- Selected items ---

    private Project? _selectedProject;
    public Project? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (SetProperty(ref _selectedProject, value))
                LoadTasksForProject(value);
        }
    }

    private ProjectTask? _selectedTask;
    public ProjectTask? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (SetProperty(ref _selectedTask, value))
            {
                LoadTimeEntriesForTask(value);
                // Refresh the Start button's enabled state when the task selection changes
                StartTimerCommand.NotifyCanExecuteChanged();
            }
        }
    }

    // --- Timer state ---

    private DispatcherTimer? _timer;
    private DateTime _timerStartedAt;
    // Captured when Start is clicked — holds the task even if the user navigates away
    private ProjectTask? _timerTask;

    private bool _isTimerRunning;
    public bool IsTimerRunning
    {
        get => _isTimerRunning;
        set
        {
            if (SetProperty(ref _isTimerRunning, value))
            {
                // Both buttons depend on this value — re-evaluate both
                StartTimerCommand.NotifyCanExecuteChanged();
                StopTimerCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _elapsedTime = "00:00:00";
    public string ElapsedTime
    {
        get => _elapsedTime;
        set => SetProperty(ref _elapsedTime, value);
    }

    private string _currentDescription = string.Empty;
    public string CurrentDescription
    {
        get => _currentDescription;
        set => SetProperty(ref _currentDescription, value);
    }

    // Shows which task the running timer belongs to — unaffected by project/task navigation
    private string _timerTaskName = "Select a task to start the timer";
    public string TimerTaskName
    {
        get => _timerTaskName;
        set => SetProperty(ref _timerTaskName, value);
    }

    // RelayCommand(action, canExecute) — canExecute controls whether the button is enabled
    public RelayCommand StartTimerCommand { get; }
    public RelayCommand StopTimerCommand { get; }

    // --- Constructor ---

    public MainWindowViewModel()
    {
        StartTimerCommand = new RelayCommand(StartTimer, () => SelectedTask is not null && !IsTimerRunning);
        StopTimerCommand  = new RelayCommand(StopTimer,  () => IsTimerRunning);
        LoadProjects();
    }

    // --- Projects ---

    private void LoadProjects()
    {
        Projects.Clear();
        foreach (var project in _projectRepository.GetAll())
            Projects.Add(project);

        SelectedProject = Projects.FirstOrDefault(project => project.Id == _selectedProject?.Id)
                          ?? Projects.FirstOrDefault();
    }

    public void AddProject(Project project)
    {
        _projectRepository.Add(project);
        LoadProjects();
        SelectedProject = Projects.FirstOrDefault(p => p.Id == project.Id);
    }

    public void UpdateProject(Project project)
    {
        _projectRepository.Update(project);
        LoadProjects();
    }

    public void DeleteProject(Project project)
    {
        _projectRepository.Delete(project.Id);
        LoadProjects();
    }

    // --- Tasks ---

    private void LoadTasksForProject(Project? project)
    {
        Tasks.Clear();
        SelectedTask = null;

        if (project is null) return;

        foreach (var task in _taskRepository.GetAllForProject(project.Id))
            Tasks.Add(task);
    }

    public void AddTask(ProjectTask task)
    {
        _taskRepository.Add(task);
        LoadTasksForProject(SelectedProject);
        SelectedTask = Tasks.FirstOrDefault(t => t.Id == task.Id);
    }

    public void UpdateTask(ProjectTask task)
    {
        _taskRepository.Update(task);
        LoadTasksForProject(SelectedProject);
    }

    public void DeleteTask(ProjectTask task)
    {
        _taskRepository.Delete(task.Id);
        LoadTasksForProject(SelectedProject);
    }

    // --- Time entries ---

    private TimeEntry? _selectedTimeEntry;
    public TimeEntry? SelectedTimeEntry
    {
        get => _selectedTimeEntry;
        set => SetProperty(ref _selectedTimeEntry, value);
    }

    public void LoadTimeEntriesForTask(ProjectTask? task)
    {
        TimeEntries.Clear();
        SelectedTimeEntry = null;

        if (task is null) return;

        foreach (var entry in _timeEntryRepository.GetAllForTask(task.Id))
            TimeEntries.Add(entry);
    }

    public void AddTimeEntry(TimeEntry entry)
    {
        _timeEntryRepository.Add(entry);
        LoadTimeEntriesForTask(SelectedTask);
    }

    public void UpdateTimeEntry(TimeEntry entry)
    {
        _timeEntryRepository.Update(entry);
        LoadTimeEntriesForTask(SelectedTask);
    }

    public void DeleteTimeEntry(TimeEntry entry)
    {
        _timeEntryRepository.Delete(entry.Id);
        LoadTimeEntriesForTask(SelectedTask);
    }

    // --- Timer ---

    private void StartTimer()
    {
        _timerTask      = SelectedTask;   // capture now — SelectedTask may change later
        _timerStartedAt = DateTime.UtcNow;
        IsTimerRunning  = true;

        TimerTaskName = _timerTask?.Name ?? "Unknown task";

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void StopTimer()
    {
        _timer?.Stop();
        _timer = null;
        IsTimerRunning = false;
        ElapsedTime    = "00:00:00";
        TimerTaskName  = "Select a task to start the timer";

        if (_timerTask is null) return; // safety: should never be null, but guard anyway

        var entry = new TimeEntry
        {
            ProjectTaskId = _timerTask.Id,    // use the captured task, not SelectedTask
            Description   = CurrentDescription,
            StartedAt     = _timerStartedAt,
            StoppedAt     = DateTime.UtcNow
        };

        _timeEntryRepository.Add(entry);

        // Only reload the time log if the user is still on the task the timer ran for
        if (SelectedTask?.Id == _timerTask.Id)
            LoadTimeEntriesForTask(SelectedTask);

        _timerTask         = null;
        CurrentDescription = string.Empty;
    }

    // Called every second by the timer — updates the displayed elapsed time
    private void OnTimerTick(object? sender, EventArgs e)
    {
        var elapsed = DateTime.UtcNow - _timerStartedAt;
        ElapsedTime = elapsed.ToString(@"hh\:mm\:ss");
    }
}
