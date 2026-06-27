using System;
using System.Globalization;
using TimeTracker.Models;

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

    // Default constructor — used for adding a new entry
    public ManualTimeEntryViewModel() { }

    // Constructor for editing an existing entry — pre-populates all fields from local time
    public ManualTimeEntryViewModel(TimeEntry entry)
    {
        var localStart = entry.StartedAt.ToLocalTime();

        Title = "Edit Time Entry";
        // DateTimeOffset(DateTime, TimeSpan) throws when Kind==Local and offset != local TZ offset.
        // SpecifyKind(Unspecified) removes that constraint so we can use offset 0 safely.
        Date        = new DateTimeOffset(DateTime.SpecifyKind(localStart.Date, DateTimeKind.Unspecified), TimeSpan.Zero);
        StartTime   = localStart.ToString("HH:mm:ss");
        EndTime     = entry.StoppedAt?.ToLocalTime().ToString("HH:mm:ss") ?? string.Empty;
        Description = entry.Description;
        AiTimeSavedHours = (entry.AiTimeSavedHours ?? 0).ToString();
    }

    // Creates a brand-new TimeEntry — used when adding
    public bool TryBuildTimeEntry(int taskId, out TimeEntry? entry)
    {
        entry = null;
        if (!TryParseRange(out var start, out var end)) return false;
        if (!TryParseAiTime(out var aiHours)) return false;

        entry = new TimeEntry
        {
            ProjectTaskId = taskId,
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
        if (!TryParseRange(out var start, out var end)) return false;
        if (!TryParseAiTime(out var aiHours)) return false;

        existing.StartedAt   = start;
        existing.StoppedAt   = end;
        existing.Description = Description;
        existing.AiTimeSavedHours = aiHours;
        return true;
    }

    // Returns null when the time range is valid, or an error message the dialog can display.
    public string? Validate()
    {
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
