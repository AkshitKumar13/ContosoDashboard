using ContosoDashboard.Models;

namespace ContosoDashboard.Tests;

public class DocumentAuthorizationTests
{
    [Fact]
    public void DirectShareTargetIsIndependentFromProjectMembership()
    {
        var target = new DocumentShare { DocumentId = 1, SharedWithUserId = 4, SharedByUserId = 2 };
        Assert.Equal(4, target.SharedWithUserId);
        Assert.Null(target.SharedWithDepartment);
    }

    [Fact]
    public void DepartmentShareUsesExistingDepartmentValue()
    {
        var target = new DocumentShare { DocumentId = 1, SharedWithDepartment = "Engineering", SharedByUserId = 2 };
        Assert.Equal("Engineering", target.SharedWithDepartment);
        Assert.Null(target.SharedWithUserId);
    }
}
