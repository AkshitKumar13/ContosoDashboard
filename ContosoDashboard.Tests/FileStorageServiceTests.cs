using ContosoDashboard.Services;
using Microsoft.Extensions.Options;

namespace ContosoDashboard.Tests;

public class FileStorageServiceTests
{
    [Fact]
    public void StorageOptionsUseApplicationDataDefaults()
    {
        var options = new DocumentStorageOptions();
        Assert.Equal("AppData/uploads", options.RootPath);
        Assert.Equal(DocumentValidation.MaxFileSizeBytes, options.MaxFileSizeBytes);
    }

    [Fact]
    public void RelativeStoragePathContractIsConfigured()
    {
        Assert.False(Path.IsPathRooted("3/personal/file.txt"));
        Assert.True(DocumentValidation.GetExtension("file.txt") == ".txt");
    }
}
