namespace ContosoDashboard.Services;

public enum FileScanStatus
{
    Clean,
    Unsafe,
    Unavailable
}

public sealed record FileScanResult(FileScanStatus Status, string? Message = null);

public interface IFileScanner
{
    Task<FileScanResult> ScanAsync(Stream content, string contentType, CancellationToken cancellationToken = default);
}

public sealed class TrainingFileScanner : IFileScanner
{
    private const string TestSignature = "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";

    public async Task<FileScanResult> ScanAsync(Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        if (!content.CanRead) return new FileScanResult(FileScanStatus.Unavailable, "The file could not be scanned.");

        var originalPosition = content.CanSeek ? content.Position : 0;
        try
        {
            if (content.CanSeek) content.Position = 0;
            using var reader = new StreamReader(content, leaveOpen: true);
            var text = await reader.ReadToEndAsync(cancellationToken);
            return text.Contains(TestSignature, StringComparison.Ordinal)
                ? new FileScanResult(FileScanStatus.Unsafe, "The file failed the malware scan.")
                : new FileScanResult(FileScanStatus.Clean);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new FileScanResult(FileScanStatus.Unavailable, "The file could not be scanned.");
        }
        finally
        {
            if (content.CanSeek) content.Position = originalPosition;
        }
    }
}
