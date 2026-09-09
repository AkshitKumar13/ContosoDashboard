using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Tests;

public static class DocumentTestFixture
{
    public static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        context.Users.AddRange(
            new User { UserId = 1, DisplayName = "Administrator", Email = "admin@test.local", Department = "IT", Role = UserRole.Administrator },
            new User { UserId = 2, DisplayName = "Project Manager", Email = "pm@test.local", Department = "Engineering", Role = UserRole.ProjectManager },
            new User { UserId = 3, DisplayName = "Employee", Email = "employee@test.local", Department = "Engineering", Role = UserRole.Employee },
            new User { UserId = 4, DisplayName = "Other", Email = "other@test.local", Department = "Finance", Role = UserRole.Employee });
        context.Projects.Add(new Project { ProjectId = 1, Name = "Test Project", ProjectManagerId = 2 });
        context.ProjectMembers.Add(new ProjectMember { ProjectMemberId = 1, ProjectId = 1, UserId = 3, Role = "Developer" });
        context.SaveChanges();
        return context;
    }
}
