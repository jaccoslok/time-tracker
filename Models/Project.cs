using System;
using System.Collections.Generic;

namespace TimeTracker.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property — EF Core uses this to load related tasks
    public List<ProjectTask> Tasks { get; set; } = [];
}
