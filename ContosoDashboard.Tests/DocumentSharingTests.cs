using ContosoDashboard.Models;

namespace ContosoDashboard.Tests;

public class DocumentSharingTests
{
    [Fact]
    public void SharingActivityCapturesShareAction()
    {
        var activity = new DocumentActivity { ActorUserId = 2, Action = "Share", Details = "Engineering" };
        Assert.Equal("Share", activity.Action);
        Assert.Equal("Engineering", activity.Details);
    }

    [Fact]
    public void DocumentNotificationTypesAreAvailable()
    {
        Assert.Equal("DocumentShared", NotificationType.DocumentShared.ToString());
        Assert.Equal("DocumentAddedToProject", NotificationType.DocumentAddedToProject.ToString());
    }
}
