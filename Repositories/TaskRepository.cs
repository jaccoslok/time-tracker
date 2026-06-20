using System.Collections.Generic;
using System.Linq;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Repositories;

public class TaskRepository
{
    public List<ProjectTask> GetAllForProject(int projectId)
    {
        using var db = new AppDbContext();
        return db.ProjectTasks
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
