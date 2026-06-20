using System;
using System.Collections.ObjectModel;
using System.Linq;
using TimeTracker.Repositories;

namespace TimeTracker.ViewModels;

// One row in the summary list — represents a single time entry
public class SummaryRow
{
    public string Date        { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string TaskName    { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StartTime   { get; set; } = string.Empty;
    public string StopTime    { get; set; } = string.Empty;
    public string Duration    { get; set; } = string.Empty;
}

public class SummaryViewModel : ViewModelBase
{
    private readonly TimeEntryRepository _timeEntryRepository = new();

    public ObservableCollection<SummaryRow> Rows { get; } = [];

    // Default range: current week (last 7 days)
    private DateTimeOffset _fromDate = DateTimeOffset.Now.Date.AddDays(-6);
    public DateTimeOffset FromDate
    {
        get => _fromDate;
        set
        {
            if (SetProperty(ref _fromDate, value))
                LoadSummary();
        }
    }

    private DateTimeOffset _toDate = DateTimeOffset.Now.Date;
    public DateTimeOffset ToDate
    {
        get => _toDate;
        set
        {
            if (SetProperty(ref _toDate, value))
                LoadSummary();
        }
    }

    private double _totalHours;
    public double TotalHours
    {
        get => _totalHours;
        set => SetProperty(ref _totalHours, value);
    }

    public SummaryViewModel()
    {
        LoadSummary();
    }

    private void LoadSummary()
    {
        Rows.Clear();

        var from = FromDate.Date;
        var to   = ToDate.Date.AddDays(1); // include the full 'to' day

        var entries = _timeEntryRepository.GetAllInRange(from, to);

        // One row per entry, ordered by date then project then task
        var rows = entries
            .OrderBy(entry => entry.StartedAt)
            .ThenBy(entry => entry.ProjectTask.Project.Name)
            .ThenBy(entry => entry.ProjectTask.Name)
            .Select(entry => new SummaryRow
            {
                Date        = entry.StartedAt.ToLocalTime().ToString("dd-MM-yyyy"),
                ProjectName = entry.ProjectTask.Project.Name,
                TaskName    = entry.ProjectTask.Name,
                Description = entry.Description,
                StartTime   = entry.StartedAt.ToLocalTime().ToString("HH:mm"),
                StopTime    = entry.StoppedAt?.ToLocalTime().ToString("HH:mm") ?? "—",
                Duration    = FormatDuration(entry.Duration)
            });

        foreach (var row in rows)
            Rows.Add(row);

        TotalHours = entries.Sum(entry => entry.Duration?.TotalHours ?? 0);
    }

    // Converts a nullable TimeSpan to "h:mm", e.g. 1h 45m → "1:45"
    private static string FormatDuration(TimeSpan? duration)
    {
        if (duration is null) return "—";
        return $"{(int)duration.Value.TotalHours}:{duration.Value.Minutes:D2}";
    }
}
