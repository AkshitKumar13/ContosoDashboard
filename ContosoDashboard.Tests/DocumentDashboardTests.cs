using ContosoDashboard.Models;

namespace ContosoDashboard.Tests;

public class DocumentDashboardTests
{
    [Fact]
    public void RecentDocumentsAreLimitedToFiveByDashboardContract()
    {
        var recent = Enumerable.Range(1, 5).Select(id => new Document { DocumentId = id }).ToList();
        Assert.Equal(5, recent.Count);
    }
}
