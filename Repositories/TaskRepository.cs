using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Repositories;

public class TaskRepository
{
    public List<ProjectTask> GetAllForProject(int projectId)
    {
        using var db = new AppDbContext();
        return db.ProjectTasks
            .AsNoTracking()  // read-only — no need to track changes
            .Where(task => task.ProjectId == projectId && task.IsActive)
            .OrderBy(task => task.Name)
            .ToList();
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
        db.ProjectTasks.Update(task);
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
