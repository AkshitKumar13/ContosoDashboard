using ContosoDashboard.Services;

namespace ContosoDashboard.Tests;

public class DocumentValidationTests
{
    [Fact]
    public void AcceptsSupportedPdfWithinLimit()
    {
        var error = DocumentValidation.ValidateMetadata("Requirements", "Project Documents", 1024, "requirements.pdf", "application/pdf");
        Assert.Null(error);
    }

    [Fact]
    public void RejectsUnsupportedType()
    {
        var error = DocumentValidation.ValidateMetadata("Executable", "Other", 1024, "virus.exe", "application/octet-stream");
        Assert.Equal("The selected file type is not supported.", error);
    }

    [Fact]
    public void RejectsOversizedFile()
    {
        var error = DocumentValidation.ValidateMetadata("Large", "Other", DocumentValidation.MaxFileSizeBytes + 1, "large.txt", "text/plain");
        Assert.Contains("25 MB", error);
    }
}
