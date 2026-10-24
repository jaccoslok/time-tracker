using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TimeTracker.Models;
using TimeTracker.Repositories;

namespace TimeTracker.ViewModels;

public class ManualTimeEntryViewModel : ViewModelBase
{
    private string _title = "Add Time Entry";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private DateTimeOffset _date = DateTimeOffset.Now;
    public DateTimeOffset Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    private string _startTime = string.Empty;
    public string StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }

    private string _endTime = string.Empty;
    public string EndTime
    {
        get => _endTime;
        set => SetProperty(ref _endTime, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private string _aiTimeSavedHours = "0";
    public string AiTimeSavedHours
    {
        get => _aiTimeSavedHours;
        set => SetProperty(ref _aiTimeSavedHours, value);
    }

    // --- Project and task pickers ---

    private readonly ProjectRepository _projectRepository = new();
    private readonly TaskRepository _taskRepository = new();

    public ObservableCollection<Project> Projects { get; } = [];
    public ObservableCollection<ProjectTask> Tasks { get; } = [];

    private Project? _selectedProject;
    public Project? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (SetProperty(ref _selectedProject, value))
                LoadTasks(selectTaskId: 0);
        }
    }

    private ProjectTask? _selectedTask;
    public ProjectTask? SelectedTask
    {
        get => _selectedTask;
        set => SetProperty(ref _selectedTask, value);
    }

    // Parameterless constructor — only for the XAML previewer (Design.DataContext)
    public ManualTimeEntryViewModel() { }

    // Constructor for adding a new entry; defaultTaskId (0 = none) preselects a task.
    // Defaults to the hour that just ended, as long as it falls on today.
    public ManualTimeEntryViewModel(int defaultTaskId)
    {
        var now = DateTime.Now;
        EndTime = now.ToString("HH:mm");
        if (now.Hour >= 1)
            StartTime = now.AddHours(-1).ToString("HH:mm");

        LoadPickers(defaultTaskId);
    }

    // Constructor for editing an existing entry — pre-populates all fields from local time
    public ManualTimeEntryViewModel(TimeEntry entry)
    {
        var localStart = entry.StartedAt.ToLocalTime();

        LoadPickers(entry.ProjectTaskId);
        Title = "Edit Time Entry";
        // DateTimeOffset(DateTime, TimeSpan) throws when Kind==Local and offset != local TZ offset.
        // SpecifyKind(Unspecified) removes that constraint so we can use offset 0 safely.
        Date        = new DateTimeOffset(DateTime.SpecifyKind(localStart.Date, DateTimeKind.Unspecified), TimeSpan.Zero);
        StartTime   = localStart.ToString("HH:mm:ss");
        EndTime     = entry.StoppedAt?.ToLocalTime().ToString("HH:mm:ss") ?? string.Empty;
        Description = entry.Description;
        AiTimeSavedHours = (entry.AiTimeSavedHours ?? 0).ToString();
    }

    // Fills the pickers; closed projects and tasks are included so old entries can be edited
    private void LoadPickers(int taskId)
    {
        var task = taskId > 0 ? _taskRepository.GetById(taskId) : null;

        foreach (var project in _projectRepository.GetAll(includeClosed: true))
            Projects.Add(project);

        // Set the backing field directly, so the setter doesn't load the tasks a second time
        _selectedProject = Projects.FirstOrDefault(project => project.Id == task?.ProjectId)
                           ?? Projects.FirstOrDefault();
        LoadTasks(taskId);
    }

    private void LoadTasks(int selectTaskId)
    {
        Tasks.Clear();
        if (SelectedProject is not null)
        {
            foreach (var task in _taskRepository.GetAllForProject(SelectedProject.Id, includeClosed: true))
                Tasks.Add(task);
        }
        SelectedTask = Tasks.FirstOrDefault(task => task.Id == selectTaskId) ?? Tasks.FirstOrDefault();
    }

    // Creates a brand-new TimeEntry — used when adding
    public bool TryBuildTimeEntry(out TimeEntry? entry)
    {
        entry = null;
        if (SelectedTask is null) return false;
        if (!TryParseRange(out var start, out var end)) return false;
        if (!TryParseAiTime(out var aiHours)) return false;

        entry = new TimeEntry
        {
            ProjectTaskId = SelectedTask.Id,
            Description   = Description,
            StartedAt     = start,
            StoppedAt     = end,
            AiTimeSavedHours = aiHours
        };
        return true;
    }

    // Applies edited values to an existing TimeEntry in place — used when editing
    public bool TryApplyToEntry(TimeEntry existing)
    {
        if (SelectedTask is null) return false;
        if (!TryParseRange(out var start, out var end)) return false;
        if (!TryParseAiTime(out var aiHours)) return false;

        existing.ProjectTaskId = SelectedTask.Id;
        existing.StartedAt   = start;
        existing.StoppedAt   = end;
        existing.Description = Description;
        existing.AiTimeSavedHours = aiHours;
        return true;
    }

    // Returns null when the time range is valid, or an error message the dialog can display.
    public string? Validate()
    {
        if (SelectedTask is null)
            return "Choose a project and task for this time entry.";
        if (!TimeSpan.TryParse(StartTime, out var startSpan))
            return "Start time is not valid. Use HH:mm or HH:mm:ss (e.g. 09:00).";
        if (!TimeSpan.TryParse(EndTime, out var endSpan))
            return "End time is not valid. Use HH:mm or HH:mm:ss (e.g. 17:30).";
        if (endSpan < startSpan)
            return "End time must be later than start time.";
        if (!TryParseAiTime(out _))
            return "AI time must be a valid decimal number (e.g. 0, 0.5, 1.25).";
        return null;
    }

    private bool TryParseAiTime(out decimal? aiHours)
    {
        aiHours = null;
        if (string.IsNullOrWhiteSpace(AiTimeSavedHours) || AiTimeSavedHours == "0")
            return true;  // Empty or 0 is allowed
        
        // Try to parse using current culture's decimal separator
        if (decimal.TryParse(AiTimeSavedHours, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out var value) && value >= 0)
        {
            aiHours = value;
            return true;
        }
        return false;
    }

    private bool TryParseRange(out DateTime start, out DateTime end)
    {
        start = default;
        end   = default;

        if (!TimeSpan.TryParse(StartTime, out var startSpan)) return false;
        if (!TimeSpan.TryParse(EndTime,   out var endSpan))   return false;
        if (endSpan < startSpan) return false;

        // Date.Date gives the selected date as Kind.Unspecified.
        // SpecifyKind → Local tells ToUniversalTime() to treat the value as local
        // time and subtract the local offset — producing a UTC value with Kind.Utc,
        // consistent with how the timer stores StartedAt and StoppedAt.
        var localDate = Date.Date;
        start = DateTime.SpecifyKind(localDate + startSpan, DateTimeKind.Local).ToUniversalTime();
        end   = DateTime.SpecifyKind(localDate + endSpan,   DateTimeKind.Local).ToUniversalTime();
        return true;
    }
}
