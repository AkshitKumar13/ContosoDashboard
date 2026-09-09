using Microsoft.Extensions.Options;

namespace ContosoDashboard.Services;

public sealed class DocumentStorageOptions
{
    public string RootPath { get; set; } = "AppData/uploads";
    public long MaxFileSizeBytes { get; set; } = DocumentValidation.MaxFileSizeBytes;
}

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream content, string relativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string relativePath, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default);
}

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IOptions<DocumentStorageOptions> options, IWebHostEnvironment environment)
    {
        var configuredRoot = options.Value.RootPath;
        _rootPath = Path.IsPathFullyQualified(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> UploadAsync(Stream content, string relativePath, CancellationToken cancellationToken = default)
    {
        var safePath = GetSafePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(safePath)!);
        await using var file = new FileStream(safePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await content.CopyToAsync(file, cancellationToken);
        return NormalizeRelativePath(relativePath);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var path = GetSafePath(relativePath);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public Task<Stream> DownloadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var path = GetSafePath(relativePath);
        if (!File.Exists(path)) throw new FileNotFoundException("The document file was not found.");
        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(File.Exists(GetSafePath(relativePath)));
    }

    private string GetSafePath(string relativePath)
    {
        var normalized = NormalizeRelativePath(relativePath);
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, normalized.Replace('/', Path.DirectorySeparatorChar)));
        var root = Path.GetFullPath(_rootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The storage path is outside the configured storage root.", nameof(relativePath));
        return fullPath;
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            throw new ArgumentException("A relative storage path is required.", nameof(relativePath));

        var normalized = relativePath.Replace('\\', '/').Trim('/');
        if (normalized.Split('/').Any(segment => segment is "" or "." or ".."))
            throw new ArgumentException("The storage path contains an unsafe segment.", nameof(relativePath));
        return normalized;
    }
}
