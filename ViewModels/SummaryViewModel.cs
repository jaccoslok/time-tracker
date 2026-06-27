using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using TimeTracker.Repositories;

namespace TimeTracker.ViewModels;

// One row in the summary list — represents a single time entry
public class SummaryRow
{
    public int    Id                { get; set; }
    public string Date              { get; set; } = string.Empty;
    public string ProjectName       { get; set; } = string.Empty;
    public string TaskName          { get; set; } = string.Empty;
    public string Description       { get; set; } = string.Empty;
    public string StartTime         { get; set; } = string.Empty;
    public string StopTime          { get; set; } = string.Empty;
    public string Duration          { get; set; } = string.Empty;
    public string AiTimeSavedHours  { get; set; } = string.Empty;
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

    private decimal _totalAiTimeSaved;
    public decimal TotalAiTimeSaved
    {
        get => _totalAiTimeSaved;
        set => SetProperty(ref _totalAiTimeSaved, value);
    }

    public SummaryViewModel()
    {
        // Skip DB access when the Avalonia XAML previewer instantiates this ViewModel.
        if (!Design.IsDesignMode)
            LoadSummary();
    }

    public void SetTodayRange()
    {
        var today = DateTimeOffset.Now.Date;

        _fromDate = today;
        _toDate = today;

        OnPropertyChanged(nameof(FromDate));
        OnPropertyChanged(nameof(ToDate));

        LoadSummary();
    }

    private void LoadSummary()
    {
        Rows.Clear();

        var from = FromDate.Date;
        var to   = ToDate.Date.AddDays(1); // include the full 'to' day

        var entries = _timeEntryRepository.GetAllInRange(from, to);

        // Sorting is now handled by the repository query (pushed to SQLite).
        var rows = entries
            .Select(entry => new SummaryRow
            {
                Id                = entry.Id,
                Date              = entry.StartedAt.ToLocalTime().ToString("dd-MM-yyyy"),
                ProjectName       = entry.ProjectTask.Project.Name,
                TaskName          = entry.ProjectTask.Name,
                Description       = entry.Description,
                StartTime         = entry.StartedAt.ToLocalTime().ToString("HH:mm"),
                StopTime          = entry.StoppedAt?.ToLocalTime().ToString("HH:mm") ?? "—",
                Duration          = FormatDuration(entry.Duration),
                AiTimeSavedHours  = FormatAiTime(entry.AiTimeSavedHours)
            });

        foreach (var row in rows)
            Rows.Add(row);

        TotalHours = entries.Sum(entry => entry.Duration?.TotalHours ?? 0);
        TotalAiTimeSaved = entries.Sum(entry => entry.AiTimeSavedHours ?? 0m);
    }

    // Called from the view after an entry is edited so the list refreshes
    public void Reload() => LoadSummary();

    // Converts a nullable TimeSpan to "h:mm", e.g. 1h 45m → "1:45"
    private static string FormatDuration(TimeSpan? duration)
    {
        if (duration is null) return "—";
        return $"{(int)duration.Value.TotalHours}:{duration.Value.Minutes:D2}";
    }

    // Formats AI time saved: returns "—" if null or 0, otherwise the decimal value with 1 decimal place
    private static string FormatAiTime(decimal? aiHours)
    {
        if (aiHours is null or 0) return "—";
        return aiHours.Value.ToString("F1");
    }
}
