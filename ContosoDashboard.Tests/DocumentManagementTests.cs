using ContosoDashboard.Services;

namespace ContosoDashboard.Tests;

public class DocumentManagementTests
{
    [Fact]
    public void ReplacementUsesTheSameSupportedFileRules()
    {
        Assert.Null(DocumentValidation.ValidateMetadata("Existing", "Other", 100, "updated.pdf", "application/pdf"));
        Assert.NotNull(DocumentValidation.ValidateMetadata("Existing", "Other", 100, "updated.exe", "application/octet-stream"));
    }
}
