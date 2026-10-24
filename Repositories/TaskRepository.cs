using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Repositories;

public class TaskRepository
{
    // Tasks of one project, or of all projects when projectId is 0.
    // The parent Project is included: lists show its code and rates fall back to it.
    public List<ProjectTask> GetAllForProject(int projectId, bool includeClosed = false)
    {
        using var db = new AppDbContext();
        return db.ProjectTasks
            .AsNoTracking()  // read-only — no need to track changes
            .Include(task => task.Project)
            .Where(task => projectId == 0 || task.ProjectId == projectId)
            .Where(task => includeClosed || task.IsActive)
            .OrderBy(task => task.Project.Code)
            .ThenBy(task => task.Name)
            .ToList();
    }

    public ProjectTask? GetById(int id)
    {
        using var db = new AppDbContext();
        return db.ProjectTasks.AsNoTracking().FirstOrDefault(task => task.Id == id);
    }

    public void Add(ProjectTask task)
    {
        using var db = new AppDbContext();
        db.ProjectTasks.Add(task);
        db.SaveChanges();
    }

    public void Update(ProjectTask task)
    {
        using var db = new AppDbContext();
        // Entry(...).State marks only the task as modified. Update() would also walk the
        // loaded Project navigation and write that project back to the database.
        db.Entry(task).State = EntityState.Modified;
        db.SaveChanges();
    }

    public void Delete(int id)
    {
        using var db = new AppDbContext();
        var task = db.ProjectTasks.Find(id);
        if (task is not null)
        {
            db.ProjectTasks.Remove(task);
            db.SaveChanges();
        }
    }
}
