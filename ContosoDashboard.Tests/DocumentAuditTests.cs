using ContosoDashboard.Models;

namespace ContosoDashboard.Tests;

public class DocumentAuditTests
{
    [Fact]
    public void AuditRecordHasAttributableActorAndTimestamp()
    {
        var activity = new DocumentActivity { ActorUserId = 2, Action = "Download", CreatedDate = DateTime.UtcNow };
        Assert.True(activity.ActorUserId > 0);
        Assert.NotEqual(default, activity.CreatedDate);
    }
}
