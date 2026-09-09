using System.Text;
using ContosoDashboard.Services;

namespace ContosoDashboard.Tests;

public class FileScannerServiceTests
{
    [Fact]
    public async Task CleanContentIsAccepted()
    {
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("safe document"));
        var result = await new TrainingFileScanner().ScanAsync(content, "text/plain");
        Assert.Equal(FileScanStatus.Clean, result.Status);
    }

    [Fact]
    public async Task TestMalwareSignatureIsRejected()
    {
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*"));
        var result = await new TrainingFileScanner().ScanAsync(content, "text/plain");
        Assert.Equal(FileScanStatus.Unsafe, result.Status);
    }
}
