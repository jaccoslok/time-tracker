using System;
using TimeTracker.Helpers;
using TimeTracker.Models;

namespace TimeTracker.ViewModels;

// One row in the Projects list. Project is null for the "(All projects)" row.
// A record is a compact class for data; the constructor parameter becomes a property.
public record ProjectListItem(Project? Project)
{
    public bool IsAllRow => Project is null;
    public bool IsClosed => Project is { IsActive: false };  // property pattern: not null AND IsActive == false
    public string Code => Project?.Code ?? string.Empty;
    public string Customer => Project?.Customer ?? "(All projects)";
    public string Description => Project?.Description ?? string.Empty;
}

// One row in the Tasks list. Task is null for the "(All tasks)" row.
public record TaskListItem(ProjectTask? Task, bool ShowProjectCode, double LoggedHours)
{
    public bool IsAllRow => Task is null;
    public bool IsClosed => Task is { IsActive: false };

    public string Name => Task is null ? "(All tasks)"
        : ShowProjectCode ? $"{Task.Project.ShortName} · {Task.Name}"
        : Task.Name;

    public string Estimated => Task is { EstimatedHours: > 0 } ? DisplayFormat.Hours(Task.EstimatedHours) : string.Empty;
    public string Logged => Task is null ? string.Empty : DisplayFormat.Hours(LoggedHours);
}

// One row in the Activities list. Needs ProjectTask and ProjectTask.Project to be loaded.
public class ActivityRow
{
    public ActivityRow(TimeEntry entry)
    {
        var task = entry.ProjectTask;

        Entry        = entry;
        Duration     = entry.Duration ?? TimeSpan.Zero;
        IsBillable   = task.IsEffectivelyBillable;
        IsProductive = task.IsEffectivelyProductive;
        HourlyRate   = task.EffectiveHourlyRate;
        Amount       = IsBillable ? (decimal)Duration.TotalHours * HourlyRate : 0;
    }

    public TimeEntry Entry { get; }
    public int Id => Entry.Id;
    public TimeSpan Duration { get; }
    public double Hours => Duration.TotalHours;
    public bool IsBillable { get; }
    public bool IsProductive { get; }
    public decimal HourlyRate { get; }
    public decimal Amount { get; }
    public decimal AiTimeSavedHours => Entry.AiTimeSavedHours ?? 0;

    public DateTime StartedAtLocal => Entry.StartedAtLocal;
    public Project Project => Entry.ProjectTask.Project;
    public string ProjectName => Project.ShortName;
    public string TaskName => Entry.ProjectTask.Name;
    public string Remarks => Entry.Description;

    // Text for the list columns
    public string DateText => StartedAtLocal.ToString("dd-MM-yyyy");
    public string StartText => StartedAtLocal.ToString("HH:mm");
    public string EndText => Entry.StoppedAtLocal?.ToString("HH:mm") ?? string.Empty;
    public string DurationText => DisplayFormat.Duration(Duration);
    public string AiTimeSavedText => AiTimeSavedHours > 0 ? AiTimeSavedHours.ToString("F1") : string.Empty;
    public string AmountText => IsBillable ? DisplayFormat.Money(Amount) : string.Empty;
}

// One card in the Summary column. The two flags colour the detail line red or green.
public record SummaryCard(string Caption, string Value, string Detail,
                          bool IsBelowMinimum = false, bool IsOnTarget = false);

// One bar in the chart; the bar is drawn as a ProgressBar from 0 to Maximum
public record ChartBar(string Label, double Value, double Maximum, string ValueText);
