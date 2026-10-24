using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Repositories;

public class TimeEntryRepository
{
    // Completed entries that started within [fromUtc, toUtc), including task and project via JOIN.
    // projectId / taskId = 0 means "any project" / "any task".
    public List<TimeEntry> GetCompleted(DateTime fromUtc, DateTime toUtc, int projectId = 0, int taskId = 0)
    {
        using var db = new AppDbContext();
        return db.TimeEntries
            .AsNoTracking()  // read-only — no need to track changes
            .Include(entry => entry.ProjectTask)
            .ThenInclude(task => task.Project)
            .Where(entry => entry.StoppedAt.HasValue
                         && entry.StartedAt >= fromUtc
                         && entry.StartedAt < toUtc)
            .Where(entry => projectId == 0 || entry.ProjectTask.ProjectId == projectId)
            .Where(entry => taskId == 0 || entry.ProjectTaskId == taskId)
            .OrderByDescending(entry => entry.StartedAt)  // newest first — pushed to DB
            .ToList();
    }

    // Total logged hours per task Id, over all time
    public Dictionary<int, double> GetLoggedHoursByTask()
    {
        using var db = new AppDbContext();
        // EF Core cannot translate date subtraction for SQLite, so fetch only the
        // timestamps (Select keeps the query small) and add them up in memory.
        return db.TimeEntries
            .AsNoTracking()
            .Where(entry => entry.StoppedAt.HasValue)
            .Select(entry => new { entry.ProjectTaskId, entry.StartedAt, entry.StoppedAt })
            .AsEnumerable()  // everything after this line runs in C#, not in SQL
            .GroupBy(entry => entry.ProjectTaskId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(entry => (entry.StoppedAt!.Value - entry.StartedAt).TotalHours));
    }

    public int CountForProject(int projectId)
    {
        using var db = new AppDbContext();
        return db.TimeEntries.Count(entry => entry.ProjectTask.ProjectId == projectId);
    }

    public int CountForTask(int taskId)
    {
        using var db = new AppDbContext();
        return db.TimeEntries.Count(entry => entry.ProjectTaskId == taskId);
    }

    // Loads the entry without its navigation properties, ready to edit and pass to Update()
    public TimeEntry? GetById(int id)
    {
        using var db = new AppDbContext();
        return db.TimeEntries.Find(id);
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
        db.Attach(entry).State = EntityState.Modified;
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
