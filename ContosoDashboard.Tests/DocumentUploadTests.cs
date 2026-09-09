using ContosoDashboard.Services;

namespace ContosoDashboard.Tests;

public class DocumentUploadTests
{
    [Fact]
    public void UploadMetadataRequiresTitleAndCategory()
    {
        Assert.NotNull(DocumentValidation.ValidateMetadata(null, "Other", 10, "file.txt", "text/plain"));
        Assert.NotNull(DocumentValidation.ValidateMetadata("Title", null, 10, "file.txt", "text/plain"));
    }

    [Fact]
    public void UploadMetadataAcceptsOfficeMimeTypesWithLongValues()
    {
        var error = DocumentValidation.ValidateMetadata("Report", "Reports", 10, "report.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        Assert.Null(error);
    }
}
