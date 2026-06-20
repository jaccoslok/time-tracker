using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Models;

namespace TimeTracker.Data;

public class AppDbContext : DbContext
{
    // Each DbSet maps to one table in the SQLite database
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Store the database file in the OS app-data folder, not next to the executable
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string dbFolder = Path.Combine(appData, "TimeTracker");
        Directory.CreateDirectory(dbFolder); // creates the folder if it doesn't exist yet
        string dbPath = Path.Combine(dbFolder, "timetracker.db");

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }
}
