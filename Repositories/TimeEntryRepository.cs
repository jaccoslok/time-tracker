using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Repositories;

public class TimeEntryRepository
{
    // Loads all completed entries in the date range, including project and task names via JOIN
    public List<TimeEntry> GetAllInRange(DateTime from, DateTime to)
    {
        using var db = new AppDbContext();
        return db.TimeEntries
            .Include(entry => entry.ProjectTask)
            .ThenInclude(task => task.Project)
            .Where(entry => entry.StoppedAt.HasValue
                         && entry.StartedAt >= from
                         && entry.StartedAt < to)
            .OrderBy(entry => entry.StartedAt)
            .ToList();
    }

    public List<TimeEntry> GetAllForTask(int taskId)
    {
        using var db = new AppDbContext();
        return db.TimeEntries
            .Where(entry => entry.ProjectTaskId == taskId)
            .OrderByDescending(entry => entry.StartedAt)
            .ToList();
    }

    public void Add(TimeEntry entry)
    {
        using var db = new AppDbContext();
        db.TimeEntries.Add(entry);
        db.SaveChanges();
    }

    public void Update(TimeEntry entry)
    {
        using var db = new AppDbContext();
        // Use Attach + explicit Modified rather than Update() so EF Core does not
        // interpret the null ProjectTask navigation as "clear the relationship".
        db.Attach(entry).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        db.SaveChanges();
    }

    public void Delete(int id)
    {
        using var db = new AppDbContext();
        var entry = db.TimeEntries.Find(id);
        if (entry is not null)
        {
            db.TimeEntries.Remove(entry);
            db.SaveChanges();
        }
    }
}
