using System;

namespace TimeTracker.Models;

public class TimeEntry
{
    public int Id { get; set; }

    // Foreign key — stores the Id of the parent ProjectTask
    public int ProjectTaskId { get; set; }
    public ProjectTask ProjectTask { get; set; } = null!;
    public required string Description { get; set; } 
    public DateTime StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }  // nullable: null means the timer is still running
    public decimal? AiTimeSavedHours { get; set; }  // Hours saved through AI assistance (optional)

    // Computed at runtime from the two timestamps; not persisted in the database
    public TimeSpan? Duration => StoppedAt.HasValue ? StoppedAt.Value - StartedAt : null;

    // UI helper for grids where binding StringFormat for TimeSpan can be brittle.
    public string DurationText => Duration?.ToString(@"hh\:mm") ?? "—";

    // Local-time helpers for display — the stored values are always UTC
    public DateTime StartedAtLocal  => StartedAt.ToLocalTime();
    public DateTime? StoppedAtLocal => StoppedAt?.ToLocalTime();

}
