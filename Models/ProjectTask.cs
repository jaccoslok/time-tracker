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
    public bool IsActive { get; set; } = true;

    // Navigation property to the time entries recorded against this task
    public List<TimeEntry> TimeEntries { get; set; } = [];
}
