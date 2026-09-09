using ContosoDashboard.Services;

namespace ContosoDashboard.Tests;

public class DocumentQueryTests
{
    [Fact]
    public void QueryDefaultsAreBoundedAndSortedByDate()
    {
        var query = new DocumentQuery();
        Assert.Equal("date", query.SortBy);
        Assert.Equal(25, query.PageSize);
        Assert.Equal(1, query.Page);
    }
}
