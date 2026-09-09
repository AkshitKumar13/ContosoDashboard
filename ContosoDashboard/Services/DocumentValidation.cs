using System.IO;

namespace ContosoDashboard.Services;

public static class DocumentValidation
{
    public const long MaxFileSizeBytes = 25 * 1024 * 1024;

    public static readonly IReadOnlySet<string> Categories = new HashSet<string>(StringComparer.Ordinal)
    {
        "Project Documents", "Team Resources", "Personal Files", "Reports", "Presentations", "Other"
    };

    private static readonly IReadOnlyDictionary<string, string[]> AllowedTypes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = ["application/pdf"],
            [".doc"] = ["application/msword"],
            [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
            [".xls"] = ["application/vnd.ms-excel"],
            [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
            [".ppt"] = ["application/vnd.ms-powerpoint"],
            [".pptx"] = ["application/vnd.openxmlformats-officedocument.presentationml.presentation"],
            [".txt"] = ["text/plain"],
            [".jpg"] = ["image/jpeg"],
            [".jpeg"] = ["image/jpeg"],
            [".png"] = ["image/png"]
        };

    public static string? ValidateMetadata(string? title, string? category, long size, string? fileName, string? contentType)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 255) return "A document title is required and must be 255 characters or fewer.";
        if (string.IsNullOrWhiteSpace(category) || !Categories.Contains(category.Trim())) return "Select a valid document category.";
        if (size <= 0 || size > MaxFileSizeBytes) return "Each file must be larger than zero and no more than 25 MB.";
        if (string.IsNullOrWhiteSpace(fileName)) return "A file name is required.";
        if (string.IsNullOrWhiteSpace(contentType) || contentType.Length > 255) return "A valid content type is required.";

        var extension = Path.GetExtension(fileName);
        if (!AllowedTypes.TryGetValue(extension, out var contentTypes) || !contentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return "The selected file type is not supported.";

        return null;
    }

    public static string NormalizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName).Trim();
        return string.IsNullOrWhiteSpace(name) ? "document" : name;
    }

    public static string GetExtension(string fileName)
    {
        return Path.GetExtension(NormalizeFileName(fileName)).ToLowerInvariant();
    }

    public static bool IsPreviewable(string contentType) =>
        contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) ||
        contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
}
