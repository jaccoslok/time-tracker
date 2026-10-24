using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeTracker.Helpers;
using TimeTracker.Models;

namespace TimeTracker.ViewModels;

// An enum is a named set of constants; a variable of this type can only hold one of these values
public enum TimerState
{
    Stopped,
    Running,
    Paused
}

// Timer part of MainWindowViewModel (see MainWindowViewModel.cs)
public partial class MainWindowViewModel
{
    // Ticks every second while running, only to refresh the clock text
    private readonly DispatcherTimer _ticker = new() { Interval = TimeSpan.FromSeconds(1) };

    private DateTime _segmentStartedAt;  // UTC start of the current running stretch
    private TimeSpan _accumulated;       // total of earlier stretches, before a pause
    private ProjectTask? _runningTask;   // the task the timer records to

    // Pickers in the timer bar: only active (not closed) projects and tasks
    public ObservableCollection<Project> TimerProjects { get; } = [];
    public ObservableCollection<ProjectTask> TimerTasks { get; } = [];

    [ObservableProperty]
    private Project? _timerProject;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartButtonText))]
    [NotifyCanExecuteChangedFor(nameof(StartTimerCommand))]
    private ProjectTask? _timerTask;

    [ObservableProperty]
    private string _remarks = string.Empty;

    [ObservableProperty]
    private string _elapsedTime = "0:00:00";

    [ObservableProperty]
    private string _timerStatus = "Not tracking";

    [ObservableProperty]
    private string _windowTitle = "Time Tracker";

    // NotifyCanExecuteChangedFor re-evaluates the buttons' enabled state when State changes
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTimerRunning), nameof(IsTimerPaused), nameof(StartButtonText))]
    [NotifyCanExecuteChangedFor(nameof(StartTimerCommand), nameof(PauseTimerCommand), nameof(StopTimerCommand))]
    private TimerState _state;

    public bool IsTimerRunning => State == TimerState.Running;
    public bool IsTimerPaused => State == TimerState.Paused;

    // While running, picking another task and pressing "Switch" closes the current
    // activity and starts the new one at the same moment
    private bool IsSwitching => State == TimerState.Running && TimerTask is not null && TimerTask.Id != _runningTask?.Id;

    public string StartButtonText => State switch
    {
        TimerState.Paused => "Resume",
        _ when IsSwitching => "Switch",
        _ => "Start"
    };

    // Lets the window refuse to delete the project/task the timer is recording to
    public bool IsTimerUsing(Project project) => State != TimerState.Stopped && _runningTask?.ProjectId == project.Id;
    public bool IsTimerUsing(ProjectTask task) => State != TimerState.Stopped && _runningTask?.Id == task.Id;

    // --- Commands ---
    // [RelayCommand] generates a "StartTimerCommand" property from this method;
    // CanExecute names the method or property that decides whether the button is enabled.

    [RelayCommand(CanExecute = nameof(CanStartTimer))]
    private void StartTimer()
    {
        var now = DateTime.UtcNow;

        if (State == TimerState.Paused)
        {
            _segmentStartedAt = now;  // resume: a new stretch for the same task
        }
        else
        {
            if (TimerTask is null) return;
            if (State == TimerState.Running)
                SaveSegment(now);     // switching: close the current activity at the same moment

            _runningTask = TimerTask;
            _accumulated = TimeSpan.Zero;
            _segmentStartedAt = now;
        }

        State = TimerState.Running;
        _ticker.Start();
        UpdateTimerDisplay();
    }

    private bool CanStartTimer() =>
        State == TimerState.Paused || (TimerTask is not null && (State == TimerState.Stopped || IsSwitching));

    [RelayCommand(CanExecute = nameof(IsTimerRunning))]
    private void PauseTimer()
    {
        var now = DateTime.UtcNow;
        _accumulated += now - _segmentStartedAt;
        SaveSegment(now);

        _ticker.Stop();
        State = TimerState.Paused;
        UpdateTimerDisplay();
    }

    [RelayCommand(CanExecute = nameof(CanStopTimer))]
    private void StopTimer()
    {
        if (State == TimerState.Running)
            SaveSegment(DateTime.UtcNow);

        _ticker.Stop();
        State = TimerState.Stopped;
        _runningTask = null;
        _accumulated = TimeSpan.Zero;
        Remarks = string.Empty;
        UpdateTimerDisplay();
    }

    private bool CanStopTimer() => State != TimerState.Stopped;

    // Stores the running stretch as a time entry
    private void SaveSegment(DateTime stoppedAtUtc)
    {
        if (_runningTask is null) return;
        // Ignore accidental clicks shorter than a second
        if (stoppedAtUtc - _segmentStartedAt < TimeSpan.FromSeconds(1)) return;

        _timeEntryRepository.Add(new TimeEntry
        {
            ProjectTaskId = _runningTask.Id,
            Description   = Remarks.Trim(),  // the remark as it is now, so it can be typed while working
            StartedAt     = _segmentStartedAt,
            StoppedAt     = stoppedAtUtc
        });

        LoadTasks();  // refreshes "Logged" hours, activities, summary and chart
    }

    // Clock text, status line and window title
    private void UpdateTimerDisplay()
    {
        var running = State == TimerState.Running ? DateTime.UtcNow - _segmentStartedAt : TimeSpan.Zero;
        ElapsedTime = DisplayFormat.Clock(_accumulated + running);

        if (State == TimerState.Stopped || _runningTask is null)
        {
            TimerStatus = "Not tracking";
            WindowTitle = "Time Tracker";
            return;
        }

        var description = $"{_runningTask.Project.DisplayName} › {_runningTask.Name}";
        TimerStatus = State == TimerState.Running
            ? $"Tracking {description} since {_segmentStartedAt.ToLocalTime():HH:mm}"
            : $"Paused: {description}";
        WindowTitle = $"Time Tracker — {description}";
    }

    // --- Timer pickers ---

    partial void OnTimerProjectChanged(Project? value)
    {
        if (_isUpdating) return;
        LoadTimerTasks();
    }

    private void SelectTimerProject(int projectId)
    {
        TimerProject = TimerProjects.FirstOrDefault(project => project.Id == projectId);
    }

    private void LoadTimerProjects()
    {
        var currentId = TimerProject?.Id ?? 0;

        _isUpdating = true;
        TimerProjects.Clear();
        foreach (var project in _projectRepository.GetAll())
            TimerProjects.Add(project);
        TimerProject = TimerProjects.FirstOrDefault(project => project.Id == currentId);
        _isUpdating = false;

        LoadTimerTasks();
    }

    private void LoadTimerTasks()
    {
        var currentId = TimerTask?.Id ?? 0;

        _isUpdating = true;
        TimerTasks.Clear();
        if (TimerProject is not null)
        {
            foreach (var task in _taskRepository.GetAllForProject(TimerProject.Id))
                TimerTasks.Add(task);
        }
        TimerTask = TimerTasks.FirstOrDefault(task => task.Id == currentId);
        _isUpdating = false;
    }
}
