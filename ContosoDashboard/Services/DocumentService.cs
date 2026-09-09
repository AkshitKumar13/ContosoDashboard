using Microsoft.EntityFrameworkCore;
using ContosoDashboard.Data;
using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public sealed class DocumentQuery
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public int? ProjectId { get; set; }
    public int? TaskId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string SortBy { get; set; } = "date";
    public bool Descending { get; set; } = true;
    public bool SharedWithMe { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class DocumentPage
{
    public IReadOnlyList<Document> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public sealed class DocumentUploadRequest
{
    public required Stream Content { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long FileSize { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required string Category { get; init; }
    public string? Tags { get; init; }
    public int? ProjectId { get; init; }
    public int? TaskId { get; init; }
}

public sealed record DocumentUploadResult(bool Succeeded, Document? Document = null, string? Error = null);
public sealed record DocumentDownloadResult(Stream Content, string FileName, string ContentType, bool IsPreviewable);
public sealed record DocumentShareTarget(int? UserId = null, string? Department = null);
public sealed record DocumentReport(IReadOnlyList<DocumentTypeReport> Types, IReadOnlyList<DocumentUploaderReport> Uploaders, IReadOnlyList<DocumentAccessReport> AccessPatterns);
public sealed record DocumentTypeReport(string ContentType, int Count);
public sealed record DocumentUploaderReport(string UploaderName, int Count);
public sealed record DocumentAccessReport(string Action, int Count);

public interface IDocumentService
{
    Task<DocumentPage> GetDocumentsAsync(int actorUserId, DocumentQuery query, CancellationToken cancellationToken = default);
    Task<DocumentPage> GetProjectDocumentsAsync(int actorUserId, int projectId, CancellationToken cancellationToken = default);
    Task<DocumentPage> GetTaskDocumentsAsync(int actorUserId, int taskId, CancellationToken cancellationToken = default);
    Task<DocumentPage> GetSharedWithMeAsync(int actorUserId, DocumentQuery query, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdAsync(int actorUserId, int documentId, CancellationToken cancellationToken = default);
    Task<DocumentUploadResult> UploadAsync(int actorUserId, DocumentUploadRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateMetadataAsync(int actorUserId, int documentId, string title, string? description, string category, string? tags, CancellationToken cancellationToken = default);
    Task<bool> ReplaceFileAsync(int actorUserId, int documentId, Stream content, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default);
    Task<DocumentDownloadResult?> DownloadAsync(int actorUserId, int documentId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int actorUserId, int documentId, CancellationToken cancellationToken = default);
    Task<bool> ShareAsync(int actorUserId, int documentId, DocumentShareTarget target, CancellationToken cancellationToken = default);
    Task<bool> RevokeShareAsync(int actorUserId, int documentShareId, CancellationToken cancellationToken = default);
    Task<DocumentReport?> GetAdminReportAsync(int actorUserId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<int> GetDocumentCountAsync(int actorUserId, CancellationToken cancellationToken = default);
    Task<List<Document>> GetRecentDocumentsAsync(int actorUserId, int count = 5, CancellationToken cancellationToken = default);
}

public sealed class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storage;
    private readonly IFileScanner _scanner;
    private readonly INotificationService _notifications;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        ApplicationDbContext context,
        IFileStorageService storage,
        IFileScanner scanner,
        INotificationService notifications,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _storage = storage;
        _scanner = scanner;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<DocumentPage> GetDocumentsAsync(int actorUserId, DocumentQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == actorUserId, cancellationToken);
        if (actor is null) return EmptyPage(query);

        var documents = AccessibleDocuments(actorUserId, actor.Department, actor.Role, query.SharedWithMe);
        return await ApplyQueryAsync(documents, query, cancellationToken);
    }

    public async Task<DocumentPage> GetProjectDocumentsAsync(int actorUserId, int projectId, CancellationToken cancellationToken = default)
    {
        if (!await CanAccessProjectAsync(actorUserId, projectId, cancellationToken)) return EmptyPage(new DocumentQuery { ProjectId = projectId });
        return await GetDocumentsAsync(actorUserId, new DocumentQuery { ProjectId = projectId }, cancellationToken);
    }

    public async Task<DocumentPage> GetTaskDocumentsAsync(int actorUserId, int taskId, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.TaskId == taskId, cancellationToken);
        if (task is null || task.ProjectId is null || !await CanAccessProjectAsync(actorUserId, task.ProjectId.Value, cancellationToken))
            return EmptyPage(new DocumentQuery());

        var query = new DocumentQuery { ProjectId = task.ProjectId, PageSize = 100 };
        var page = await GetDocumentsAsync(actorUserId, query, cancellationToken);
        return new DocumentPage
        {
            Items = page.Items.Where(d => d.TaskId == taskId).ToList(),
            TotalCount = page.Items.Count(d => d.TaskId == taskId),
            Page = page.Page,
            PageSize = page.PageSize
        };
    }

    public Task<DocumentPage> GetSharedWithMeAsync(int actorUserId, DocumentQuery query, CancellationToken cancellationToken = default)
    {
        query.SharedWithMe = true;
        return GetDocumentsAsync(actorUserId, query, cancellationToken);
    }

    public async Task<Document?> GetByIdAsync(int actorUserId, int documentId, CancellationToken cancellationToken = default)
    {
        var actor = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == actorUserId, cancellationToken);
        if (actor is null) return null;
        return await AccessibleDocuments(actorUserId, actor.Department, actor.Role)
            .Include(d => d.UploadedByUser)
            .Include(d => d.Project)
            .Include(d => d.Task)
            .Include(d => d.Shares)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
    }

    public async Task<DocumentUploadResult> UploadAsync(int actorUserId, DocumentUploadRequest request, CancellationToken cancellationToken = default)
    {
        var actor = await _context.Users.FirstOrDefaultAsync(u => u.UserId == actorUserId, cancellationToken);
        if (actor is null) return new(false, Error: "The authenticated user could not be found.");

        var validationError = DocumentValidation.ValidateMetadata(request.Title, request.Category, request.FileSize, request.FileName, request.ContentType);
        if (validationError is not null) return new(false, Error: validationError);
        if (!await CanUploadToScopeAsync(actor, request.ProjectId, request.TaskId, cancellationToken))
            return new(false, Error: "You are not authorized to upload to this project or task.");

        await using var memory = new MemoryStream();
        await request.Content.CopyToAsync(memory, cancellationToken);
        if (memory.Length > DocumentValidation.MaxFileSizeBytes) return new(false, Error: "The file exceeds the 25 MB limit.");
        memory.Position = 0;

        var scan = await _scanner.ScanAsync(memory, request.ContentType, cancellationToken);
        if (scan.Status != FileScanStatus.Clean)
            return new(false, Error: scan.Message ?? "The file could not be verified and was rejected.");

        memory.Position = 0;
        var extension = DocumentValidation.GetExtension(request.FileName);
        var scope = request.ProjectId?.ToString() ?? "personal";
        var relativePath = $"{actorUserId}/{scope}/{Guid.NewGuid():N}{extension}";
        try
        {
            await _storage.UploadAsync(memory, relativePath, cancellationToken);
            var document = new Document
            {
                Title = request.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Category = request.Category.Trim(),
                Tags = NormalizeTags(request.Tags),
                OriginalFileName = DocumentValidation.NormalizeFileName(request.FileName),
                FilePath = relativePath,
                FileSize = request.FileSize,
                ContentType = request.ContentType,
                UploadedByUserId = actorUserId,
                ProjectId = request.ProjectId,
                TaskId = request.TaskId,
                UploadedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };
            _context.Documents.Add(document);
            _context.DocumentActivities.Add(CreateActivity(actorUserId, "Upload", document, $"{document.ContentType}; {document.FileSize} bytes"));
            await _context.SaveChangesAsync(cancellationToken);
            await NotifyProjectMembersAsync(actorUserId, document, cancellationToken);
            return new(true, document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document upload failed for user {UserId}", actorUserId);
            try { await _storage.DeleteAsync(relativePath, cancellationToken); } catch (Exception cleanupEx) { _logger.LogError(cleanupEx, "Document cleanup failed for {Path}", relativePath); }
            return new(false, Error: "The document could not be stored. Please try again.");
        }
    }

    public async Task<bool> UpdateMetadataAsync(int actorUserId, int documentId, string title, string? description, string category, string? tags, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.Include(d => d.Project).Include(d => d.UploadedByUser).FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
        if (document is null || !await CanManageAsync(actorUserId, document, cancellationToken)) return false;
        var error = DocumentValidation.ValidateMetadata(title, category, document.FileSize, document.OriginalFileName, document.ContentType);
        if (error is not null) return false;
        document.Title = title.Trim();
        document.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        document.Category = category.Trim();
        document.Tags = NormalizeTags(tags);
        document.UpdatedDate = DateTime.UtcNow;
        _context.DocumentActivities.Add(CreateActivity(actorUserId, "MetadataUpdate", document, null));
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReplaceFileAsync(int actorUserId, int documentId, Stream content, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.Include(d => d.Project).Include(d => d.UploadedByUser).FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
        if (document is null || !await CanManageAsync(actorUserId, document, cancellationToken)) return false;
        var error = DocumentValidation.ValidateMetadata(document.Title, document.Category, fileSize, fileName, contentType);
        if (error is not null) return false;

        await using var memory = new MemoryStream();
        await content.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;
        var scan = await _scanner.ScanAsync(memory, contentType, cancellationToken);
        if (scan.Status != FileScanStatus.Clean) return false;
        memory.Position = 0;
        var newPath = $"{document.UploadedByUserId}/{document.ProjectId?.ToString() ?? "personal"}/{Guid.NewGuid():N}{DocumentValidation.GetExtension(fileName)}";
        await _storage.UploadAsync(memory, newPath, cancellationToken);
        var oldPath = document.FilePath;
        try
        {
            document.FilePath = newPath;
            document.OriginalFileName = DocumentValidation.NormalizeFileName(fileName);
            document.ContentType = contentType;
            document.FileSize = fileSize;
            document.UpdatedDate = DateTime.UtcNow;
            _context.DocumentActivities.Add(CreateActivity(actorUserId, "Replace", document, null));
            await _context.SaveChangesAsync(cancellationToken);
            await _storage.DeleteAsync(oldPath, cancellationToken);
            return true;
        }
        catch
        {
            try { await _storage.DeleteAsync(newPath, cancellationToken); } catch { }
            return false;
        }
    }

    public async Task<DocumentDownloadResult?> DownloadAsync(int actorUserId, int documentId, CancellationToken cancellationToken = default)
    {
        var document = await GetByIdAsync(actorUserId, documentId, cancellationToken);
        if (document is null) return null;
        var stream = await _storage.DownloadAsync(document.FilePath, cancellationToken);
        _context.DocumentActivities.Add(CreateActivity(actorUserId, "Download", document, null));
        await _context.SaveChangesAsync(cancellationToken);
        return new DocumentDownloadResult(stream, document.OriginalFileName, document.ContentType, DocumentValidation.IsPreviewable(document.ContentType));
    }

    public async Task<bool> DeleteAsync(int actorUserId, int documentId, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.Include(d => d.Project).Include(d => d.UploadedByUser).FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
        if (document is null || !await CanManageAsync(actorUserId, document, cancellationToken)) return false;
        var path = document.FilePath;
        _context.DocumentActivities.Add(CreateActivity(actorUserId, "Delete", document, null));
        _context.Documents.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);
        await _storage.DeleteAsync(path, cancellationToken);
        return true;
    }

    public async Task<bool> ShareAsync(int actorUserId, int documentId, DocumentShareTarget target, CancellationToken cancellationToken = default)
    {
        var document = await _context.Documents.Include(d => d.Project).Include(d => d.UploadedByUser).FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
        if (document is null || !await CanManageAsync(actorUserId, document, cancellationToken)) return false;
        if ((target.UserId is null) == string.IsNullOrWhiteSpace(target.Department)) return false;
        if (target.UserId is not null && !await _context.Users.AnyAsync(u => u.UserId == target.UserId, cancellationToken)) return false;
        var exists = await _context.DocumentShares.AnyAsync(s => s.DocumentId == documentId && s.SharedWithUserId == target.UserId && s.SharedWithDepartment == target.Department, cancellationToken);
        if (exists) return true;
        var share = new DocumentShare { DocumentId = documentId, SharedWithUserId = target.UserId, SharedWithDepartment = target.Department?.Trim(), SharedByUserId = actorUserId, SharedDate = DateTime.UtcNow };
        _context.DocumentShares.Add(share);
        _context.DocumentActivities.Add(CreateActivity(actorUserId, "Share", document, target.UserId?.ToString() ?? target.Department));
        await _context.SaveChangesAsync(cancellationToken);
        if (target.UserId is int recipient)
        {
            await _notifications.CreateNotificationWithRetryAsync(new Notification { UserId = recipient, Title = "Document shared with you", Message = $"{document.Title} was shared with you.", Type = NotificationType.DocumentShared, Priority = NotificationPriority.Informational }, cancellationToken);
        }
        return true;
    }

    public async Task<bool> RevokeShareAsync(int actorUserId, int documentShareId, CancellationToken cancellationToken = default)
    {
        var share = await _context.DocumentShares.Include(s => s.Document).ThenInclude(d => d.Project).Include(s => s.Document).ThenInclude(d => d.UploadedByUser).FirstOrDefaultAsync(s => s.DocumentShareId == documentShareId, cancellationToken);
        if (share is null || !await CanManageAsync(actorUserId, share.Document, cancellationToken)) return false;
        _context.DocumentShares.Remove(share);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DocumentReport?> GetAdminReportAsync(int actorUserId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        if (!await IsAdministratorAsync(actorUserId, cancellationToken)) return null;
        var cutoff = DateTime.UtcNow.AddMonths(-12);
        var from = fromDate ?? cutoff;
        var to = toDate ?? DateTime.UtcNow;
        var documents = _context.Documents.AsNoTracking().Where(d => d.UploadedDate >= from && d.UploadedDate <= to);
        var types = await documents.GroupBy(d => d.ContentType).Select(g => new DocumentTypeReport(g.Key, g.Count())).ToListAsync(cancellationToken);
        var uploaders = await documents.Include(d => d.UploadedByUser).GroupBy(d => d.UploadedByUser.DisplayName).Select(g => new DocumentUploaderReport(g.Key, g.Count())).ToListAsync(cancellationToken);
        var access = await _context.DocumentActivities.AsNoTracking().Where(a => a.CreatedDate >= from && a.CreatedDate <= to).GroupBy(a => a.Action).Select(g => new DocumentAccessReport(g.Key, g.Count())).ToListAsync(cancellationToken);
        return new DocumentReport(types, uploaders, access);
    }

    public Task<int> GetDocumentCountAsync(int actorUserId, CancellationToken cancellationToken = default)
    {
        return _context.Documents.CountAsync(d => d.UploadedByUserId == actorUserId, cancellationToken);
    }

    public Task<List<Document>> GetRecentDocumentsAsync(int actorUserId, int count = 5, CancellationToken cancellationToken = default)
    {
        return _context.Documents.AsNoTracking().Where(d => d.UploadedByUserId == actorUserId).OrderByDescending(d => d.UploadedDate).Take(Math.Clamp(count, 1, 20)).ToListAsync(cancellationToken);
    }

    private IQueryable<Document> AccessibleDocuments(int actorUserId, string? department, UserRole role, bool sharedOnly = false)
    {
        var query = _context.Documents.AsNoTracking().Include(d => d.UploadedByUser).Include(d => d.Project);
        if (role == UserRole.Administrator) return sharedOnly ? query.Where(d => d.Shares.Any(s => s.SharedWithUserId == actorUserId || s.SharedWithDepartment == department)) : query;
        var access = query.Where(d => d.UploadedByUserId == actorUserId ||
            d.ProjectId.HasValue && (d.Project!.ProjectManagerId == actorUserId || d.Project.ProjectMembers.Any(pm => pm.UserId == actorUserId)) ||
            d.Shares.Any(s => s.SharedWithUserId == actorUserId || (department != null && s.SharedWithDepartment == department)));
        return sharedOnly
            ? access.Where(d => d.Shares.Any(s => s.SharedWithUserId == actorUserId || (department != null && s.SharedWithDepartment == department)))
            : access;
    }

    private async Task<DocumentPage> ApplyQueryAsync(IQueryable<Document> query, DocumentQuery options, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            var search = options.Search.Trim();
            query = query.Where(d => d.Title.Contains(search) || (d.Description != null && d.Description.Contains(search)) || (d.Tags != null && d.Tags.Contains(search)) || d.OriginalFileName.Contains(search) || d.UploadedByUser.DisplayName.Contains(search) || (d.Project != null && d.Project.Name.Contains(search)));
        }
        if (!string.IsNullOrWhiteSpace(options.Category)) query = query.Where(d => d.Category == options.Category);
        if (options.ProjectId is int projectId) query = query.Where(d => d.ProjectId == projectId);
        if (options.TaskId is int taskId) query = query.Where(d => d.TaskId == taskId);
        if (options.FromDate is DateTime from) query = query.Where(d => d.UploadedDate >= from.Date);
        if (options.ToDate is DateTime to) query = query.Where(d => d.UploadedDate < to.Date.AddDays(1));

        query = options.SortBy.ToLowerInvariant() switch
        {
            "title" => options.Descending ? query.OrderByDescending(d => d.Title) : query.OrderBy(d => d.Title),
            "category" => options.Descending ? query.OrderByDescending(d => d.Category) : query.OrderBy(d => d.Category),
            "size" => options.Descending ? query.OrderByDescending(d => d.FileSize) : query.OrderBy(d => d.FileSize),
            _ => options.Descending ? query.OrderByDescending(d => d.UploadedDate) : query.OrderBy(d => d.UploadedDate)
        };
        var page = Math.Max(1, options.Page);
        var pageSize = Math.Clamp(options.PageSize, 1, 100);
        return new DocumentPage { Items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken), TotalCount = await query.CountAsync(cancellationToken), Page = page, PageSize = pageSize };
    }

    private async Task<bool> CanUploadToScopeAsync(User actor, int? projectId, int? taskId, CancellationToken cancellationToken)
    {
        if (taskId is int taskIdValue)
        {
            var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.TaskId == taskIdValue, cancellationToken);
            if (task?.ProjectId is null || projectId != task.ProjectId) return false;
        }
        if (projectId is null) return true;
        return actor.Role == UserRole.Administrator || await CanAccessProjectAsync(actor.UserId, projectId.Value, cancellationToken);
    }

    private async Task<bool> CanAccessProjectAsync(int actorUserId, int projectId, CancellationToken cancellationToken)
    {
        var actor = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == actorUserId, cancellationToken);
        return actor?.Role == UserRole.Administrator || await _context.Projects.AnyAsync(p => p.ProjectId == projectId && (p.ProjectManagerId == actorUserId || p.ProjectMembers.Any(pm => pm.UserId == actorUserId)), cancellationToken);
    }

    private async Task<bool> CanManageAsync(int actorUserId, Document document, CancellationToken cancellationToken)
    {
        var actor = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == actorUserId, cancellationToken);
        if (actor is null) return false;
        if (actor.Role == UserRole.Administrator || document.UploadedByUserId == actorUserId) return true;
        if (document.Project?.ProjectManagerId == actorUserId) return true;
        return actor.Role == UserRole.TeamLead && actor.Department != null && document.UploadedByUser.Department == actor.Department;
    }

    private Task<bool> IsAdministratorAsync(int actorUserId, CancellationToken cancellationToken) => _context.Users.AnyAsync(u => u.UserId == actorUserId && u.Role == UserRole.Administrator, cancellationToken);

    private async Task NotifyProjectMembersAsync(int actorUserId, Document document, CancellationToken cancellationToken)
    {
        if (document.ProjectId is not int projectId) return;
        var memberIds = await _context.ProjectMembers.Where(pm => pm.ProjectId == projectId && pm.UserId != actorUserId).Select(pm => pm.UserId).ToListAsync(cancellationToken);
        foreach (var memberId in memberIds)
        {
            await _notifications.CreateNotificationWithRetryAsync(new Notification { UserId = memberId, Title = "New project document", Message = $"A new document was added to project {projectId}.", Type = NotificationType.DocumentAddedToProject, Priority = NotificationPriority.Informational }, cancellationToken);
        }
    }

    private static DocumentActivity CreateActivity(int actorUserId, string action, Document document, string? details) => new()
    {
        DocumentId = document.DocumentId == 0 ? null : document.DocumentId,
        Document = document.DocumentId == 0 ? document : null,
        ActorUserId = actorUserId,
        Action = action,
        Details = details,
        CreatedDate = DateTime.UtcNow
    };

    private static string? NormalizeTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags)) return null;
        var values = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.OrdinalIgnoreCase).Take(20);
        return string.Join(", ", values);
    }

    private static DocumentPage EmptyPage(DocumentQuery query) => new() { Page = Math.Max(1, query.Page), PageSize = Math.Clamp(query.PageSize, 1, 100) };
}
