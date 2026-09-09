using ContosoDashboard.Services;

namespace ContosoDashboard.Tests;

public class DocumentUploadAuthorizationTests
{
    [Fact]
    public void UnsafeNamesAreReducedToDisplayNames()
    {
        Assert.Equal("secret.txt", DocumentValidation.NormalizeFileName("..\\secret.txt"));
        Assert.Equal("document", DocumentValidation.NormalizeFileName("   "));
    }

    [Fact]
    public void PreviewOnlySupportsPdfAndImages()
    {
        Assert.True(DocumentValidation.IsPreviewable("application/pdf"));
        Assert.True(DocumentValidation.IsPreviewable("image/png"));
        Assert.False(DocumentValidation.IsPreviewable("text/plain"));
    }
}
