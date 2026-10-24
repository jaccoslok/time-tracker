using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using TimeTracker.Helpers;
using TimeTracker.Models;
using TimeTracker.Repositories;

namespace TimeTracker.ViewModels;

// "partial": this class continues in MainWindowViewModel.Timer.cs and MainWindowViewModel.Summary.cs.
// The CommunityToolkit source generators also add generated code to it.
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ProjectRepository _projectRepository = new();
    private readonly TaskRepository _taskRepository = new();
    private readonly TimeEntryRepository _timeEntryRepository = new();

    // True while lists are rebuilt in code, so the selection-changed handlers ignore
    // the temporary selections that causes (Xojo's mUpdating)
    private bool _isUpdating;

    public IReadOnlyList<string> PeriodNames => Periods.Names;
    public ObservableCollection<ProjectListItem> ProjectItems { get; } = [];
    public ObservableCollection<TaskListItem> TaskItems { get; } = [];

    // [ObservableProperty] generates a public property "SelectedPeriodIndex" for this field,
    // including the PropertyChanged notification. No hand-written getter/setter needed.
    [ObservableProperty]
    private int _selectedPeriodIndex = Periods.ThisWeek;

    // [NotifyPropertyChangedFor] also raises PropertyChanged for SelectedProject,
    // because that property is calculated from this one
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedProject))]
    private ProjectListItem? _selectedProjectItem;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedTask))]
    private TaskListItem? _selectedTaskItem;

    // null when the "(All …)" row is selected
    public Project? SelectedProject => SelectedProjectItem?.Project;
    public ProjectTask? SelectedTask => SelectedTaskItem?.Task;

    public MainWindowViewModel()
    {
        _ticker.Tick += (_, _) => UpdateTimerDisplay();

        // Skip DB access when the Avalonia XAML previewer instantiates this ViewModel
        // via <Design.DataContext> — SQLite native libs are not available in the previewer.
        if (!Design.IsDesignMode)
            LoadProjects();
    }

    // --- Selection changes ---
    // The generator calls these "partial" methods after the property value changed

    partial void OnSelectedPeriodIndexChanged(int value) => LoadActivities();

    partial void OnSelectedProjectItemChanged(ProjectListItem? value)
    {
        if (_isUpdating) return;

        // When idle, picking a project in the list also prepares the timer for it
        if (State == TimerState.Stopped && value?.Project is { IsActive: true } project)
            SelectTimerProject(project.Id);

        LoadTasks();
    }

    partial void OnSelectedTaskItemChanged(TaskListItem? value)
    {
        if (_isUpdating) return;

        // When idle, picking a task in the list also prepares the timer for it
        if (State == TimerState.Stopped && value?.Task is { IsActive: true } task)
        {
            SelectTimerProject(task.ProjectId);
            TimerTask = TimerTasks.FirstOrDefault(timerTask => timerTask.Id == task.Id);
        }

        LoadActivities();
    }

    // --- Loading lists ---

    // Reloads projects, then tasks, activities and the timer pickers.
    // selectProjectId picks a row after reloading; null keeps the current selection.
    private void LoadProjects(int? selectProjectId = null)
    {
        var selectedId = selectProjectId ?? SelectedProject?.Id ?? 0;

        _isUpdating = true;
        ProjectItems.Clear();
        ProjectItems.Add(new ProjectListItem(null));
        // Closed projects stay visible (dimmed) until the "Hide closed" setting exists
        foreach (var project in _projectRepository.GetAll(includeClosed: true))
            ProjectItems.Add(new ProjectListItem(project));
        SelectedProjectItem = ProjectItems.FirstOrDefault(item => item.Project?.Id == selectedId)
                              ?? ProjectItems[0];
        _isUpdating = false;

        LoadTimerProjects();
        LoadTasks();
    }

    // Reloads the tasks of the selected project (or all tasks), then the activities
    private void LoadTasks(int? selectTaskId = null)
    {
        var selectedId = selectTaskId ?? SelectedTask?.Id ?? 0;
        var projectId = SelectedProject?.Id ?? 0;
        var loggedHours = _timeEntryRepository.GetLoggedHoursByTask();

        _isUpdating = true;
        TaskItems.Clear();
        TaskItems.Add(new TaskListItem(null, ShowProjectCode: false, LoggedHours: 0));
        foreach (var task in _taskRepository.GetAllForProject(projectId, includeClosed: true))
        {
            // With "(All projects)" selected, prefix each task with its project code
            TaskItems.Add(new TaskListItem(task, ShowProjectCode: projectId == 0,
                                           loggedHours.GetValueOrDefault(task.Id)));
        }
        SelectedTaskItem = TaskItems.FirstOrDefault(item => item.Task?.Id == selectedId)
                           ?? TaskItems[0];
        _isUpdating = false;

        LoadActivities();
    }

    // --- Projects ---

    public void AddProject(Project project)
    {
        _projectRepository.Add(project);  // EF Core fills in project.Id
        LoadProjects(selectProjectId: project.Id);
    }

    public void UpdateProject(Project project)
    {
        _projectRepository.Update(project);
        LoadProjects();
        UpdateTimerDisplay();  // the running project's name may have changed
    }

    public void DeleteProject(Project project)
    {
        _projectRepository.Delete(project.Id);  // tasks and entries go with it (cascade delete)
        LoadProjects();
    }

    public int CountTimeEntries(Project project) => _timeEntryRepository.CountForProject(project.Id);

    // --- Tasks ---

    public void AddTask(ProjectTask task)
    {
        _taskRepository.Add(task);
        LoadTimerTasks();
        LoadTasks(selectTaskId: task.Id);
    }

    public void UpdateTask(ProjectTask task)
    {
        _taskRepository.Update(task);
        LoadTimerTasks();
        LoadTasks();
        UpdateTimerDisplay();
    }

    public void DeleteTask(ProjectTask task)
    {
        _taskRepository.Delete(task.Id);
        LoadTimerTasks();
        LoadTasks();
    }

    public int CountTimeEntries(ProjectTask task) => _timeEntryRepository.CountForTask(task.Id);

    // --- Time entries ---

    public TimeEntry? GetTimeEntry(int id) => _timeEntryRepository.GetById(id);

    public void AddTimeEntry(TimeEntry entry)
    {
        _timeEntryRepository.Add(entry);
        LoadTasks();  // refreshes "Logged" hours, activities, summary and chart
        SelectedActivity = Activities.FirstOrDefault(row => row.Id == entry.Id);
    }

    public void UpdateTimeEntry(TimeEntry entry)
    {
        _timeEntryRepository.Update(entry);
        LoadTasks();
    }

    public void DeleteTimeEntry(ActivityRow row)
    {
        _timeEntryRepository.Delete(row.Id);
        LoadTasks();
    }
}
