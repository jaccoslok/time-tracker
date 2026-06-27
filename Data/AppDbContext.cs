using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Models;

namespace TimeTracker.Data;

public class AppDbContext : DbContext
{
    // Computed once at class load — the path never changes during the app's lifetime.
    // This avoids repeating the Environment/Path calls and CreateDirectory on every query.
    private static readonly string DbPath = BuildDbPath();

    private static string BuildDbPath()
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TimeTracker");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "timetracker.db");
    }

    // Each DbSet maps to one table in the SQLite database
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}
