using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Repositories;

public class ProjectRepository
{
    public List<Project> GetAll()
    {
        using var db = new AppDbContext();
        return db.Projects
            .AsNoTracking()  // read-only — no need to track changes
            .Where(project => project.IsActive)
            .OrderBy(project => project.Name)
            .ToList();
    }

    public void Add(Project project)
    {
        using var db = new AppDbContext();
        db.Projects.Add(project);
        db.SaveChanges(); // EF Core populates project.Id after this call
    }

    public void Update(Project project)
    {
        using var db = new AppDbContext();
        db.Projects.Update(project); // marks every property as modified
        db.SaveChanges();
    }

    public void Delete(int id)
    {
        using var db = new AppDbContext();
        var project = db.Projects.Find(id);
        if (project is not null)
        {
            db.Projects.Remove(project);
            db.SaveChanges();
        }
    }
}
