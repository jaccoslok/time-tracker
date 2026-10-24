using System.Collections.Generic;

namespace TimeTracker.Models;

public class ProjectTask
{
    public int Id { get; set; }

    // Foreign key — stores the Id of the parent Project
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // 0 means "no estimate"
    public double EstimatedHours { get; set; }

    // 0 means "use the project's rate"
    public decimal HourlyRate { get; set; }
    public bool IsBillable { get; set; } = true;
    public bool IsProductive { get; set; } = true;

    // false = "closed": kept for its history, but hidden from pickers
    public bool IsActive { get; set; } = true;

    // Navigation property to the time entries recorded against this task
    public List<TimeEntry> TimeEntries { get; set; } = [];

    // The three "Effective" values combine task and project settings.
    // They need Project to be loaded (.Include(task => task.Project)).
    public decimal EffectiveHourlyRate => HourlyRate > 0 ? HourlyRate : Project.HourlyRate;
    public bool IsEffectivelyBillable => IsBillable && Project.IsBillable;
    public bool IsEffectivelyProductive => IsProductive && Project.IsProductive;
}
