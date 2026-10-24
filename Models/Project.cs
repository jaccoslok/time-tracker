using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeTracker.Models;

public class Project
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // decimal (not double) for money: it stores values like 0.10 exactly
    public decimal HourlyRate { get; set; }
    public bool IsBillable { get; set; } = true;
    public bool IsProductive { get; set; } = true;

    // false = "closed": kept for its history, but hidden from pickers
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property — EF Core uses this to load related tasks
    public List<ProjectTask> Tasks { get; set; } = [];

    // "CODE · Customer · Description", skipping empty parts.
    // Get-only properties are not mapped to a column by EF Core.
    public string DisplayName => string.Join(" · ",
        new[] { Code, Customer, Description }
            .Select(part => part.Trim())
            .Where(part => part.Length > 0));

    // Short label for lists and charts: the code, or the customer when there is no code
    public string ShortName => string.IsNullOrWhiteSpace(Code) ? Customer : Code;
}
