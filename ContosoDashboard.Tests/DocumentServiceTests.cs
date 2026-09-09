using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContosoDashboard.Tests;

public class DocumentServiceTests
{
    [Fact]
    public async Task UploadStoresGeneratedPathAndMetadataForAuthorizedUser()
    {
        await using var context = CreateContext();
        context.Users.Add(new User { UserId = 10, DisplayName = "Employee", Email = "employee10@test.local", Department = "Engineering", Role = UserRole.Employee });
        await context.SaveChangesAsync();
        var storage = new MemoryStorage();
        var service = CreateService(context, storage);
        await using var content = new MemoryStream("hello"u8.ToArray());

        var result = await service.UploadAsync(10, new DocumentUploadRequest
        {
            Content = content,
            FileName = "notes.txt",
            ContentType = "text/plain",
            FileSize = content.Length,
            Title = "Notes",
            Category = "Personal Files"
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Document);
        Assert.Matches(@"^10/personal/[a-f0-9]{32}\.txt$", result.Document!.FilePath);
        Assert.Contains(result.Document.FilePath, storage.Paths);
        Assert.Single(await context.DocumentActivities.Where(a => a.Action == "Upload").ToListAsync());
    }

    [Fact]
    public async Task UploadRejectsProjectForNonMember()
    {
        await using var context = CreateContext();
        context.Users.Add(new User { UserId = 11, DisplayName = "Employee", Email = "employee11@test.local", Department = "Finance", Role = UserRole.Employee });
        context.Projects.Add(new Project { ProjectId = 20, Name = "Private Project", ProjectManagerId = 12 });
        await context.SaveChangesAsync();
        var service = CreateService(context, new MemoryStorage());
        await using var content = new MemoryStream("hello"u8.ToArray());

        var result = await service.UploadAsync(11, new DocumentUploadRequest
        {
            Content = content,
            FileName = "notes.txt",
            ContentType = "text/plain",
            FileSize = content.Length,
            Title = "Notes",
            Category = "Project Documents",
            ProjectId = 20
        });

        Assert.False(result.Succeeded);
        Assert.Contains("not authorized", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(context.Documents);
    }

    private static DocumentService CreateService(ApplicationDbContext context, MemoryStorage storage)
    {
        return new DocumentService(context, storage, new TrainingFileScanner(), new NoopNotificationService(), NullLogger<DocumentService>.Instance);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new ApplicationDbContext(options);
    }

    private sealed class MemoryStorage : IFileStorageService
    {
        public HashSet<string> Paths { get; } = [];
        private readonly Dictionary<string, byte[]> _files = [];

        public async Task<string> UploadAsync(Stream content, string relativePath, CancellationToken cancellationToken = default)
        {
            using var buffer = new MemoryStream();
            await content.CopyToAsync(buffer, cancellationToken);
            _files[relativePath] = buffer.ToArray();
            Paths.Add(relativePath);
            return relativePath;
        }

        public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default) { _files.Remove(relativePath); Paths.Remove(relativePath); return Task.CompletedTask; }
        public Task<Stream> DownloadAsync(string relativePath, CancellationToken cancellationToken = default) => Task.FromResult<Stream>(new MemoryStream(_files[relativePath]));
        public Task<bool> ExistsAsync(string relativePath, CancellationToken cancellationToken = default) => Task.FromResult(_files.ContainsKey(relativePath));
    }

    private sealed class NoopNotificationService : INotificationService
    {
        public Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false) => Task.FromResult<List<Notification>>([]);
        public Task<Notification> CreateNotificationAsync(Notification notification) => Task.FromResult(notification);
        public Task<bool> CreateNotificationWithRetryAsync(Notification notification, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> MarkAsReadAsync(int notificationId, int requestingUserId) => Task.FromResult(true);
        public Task<int> GetUnreadCountAsync(int userId) => Task.FromResult(0);
    }
}
