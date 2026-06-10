using SFA.DAS.AODP.Application.Queries.Application.Review;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Helpers.Export;

namespace SFA.DAS.AODP.Web.UnitTests.Helpers.Export;

public class ApplicationExportPathBuilderTests
{
    [Fact]
    public void GetZipFileName_ReturnsExpectedFileName()
    {
        var metadata = new ApplicationExportMetadataResponse
        {
            OrganisationName = "Test Org",
            Qan = "12345",
            SubmissionId = 42
        };

        var result = ApplicationExportPathBuilder.GetZipFileName(metadata);

        Assert.Equal("Test Org_12345_000042.zip", result);
    }

    [Fact]
    public void GetZipFileName_WhenQanMissing_UsesNoQanFolderName()
    {
        var metadata = new ApplicationExportMetadataResponse
        {
            OrganisationName = "Test Org",
            Qan = string.Empty,
            SubmissionId = 42
        };

        var result = ApplicationExportPathBuilder.GetZipFileName(metadata);

        Assert.Equal(
            $"Test Org_{ApplicationExportConstants.NoQanFolderName}_000042.zip",
            result);
    }

    [Fact]
    public void GetBasePath_ReturnsExpectedPath()
    {
        var metadata = new ApplicationExportMetadataResponse
        {
            OrganisationName = "Test Org",
            Qan = "12345",
            SubmissionId = 42,
            FormName = "My Form"
        };

        var result = ApplicationExportPathBuilder.GetBasePath(metadata);

        Assert.Equal(
            "Test Org/12345/000042_My Form",
            result);
    }

    [Fact]
    public void GetBasePath_WhenQanMissing_UsesNoQanFolderName()
    {
        var metadata = new ApplicationExportMetadataResponse
        {
            OrganisationName = "Test Org",
            Qan = null,
            SubmissionId = 42,
            FormName = "My Form"
        };

        var result = ApplicationExportPathBuilder.GetBasePath(metadata);

        Assert.Equal(
            $"Test Org/{ApplicationExportConstants.NoQanFolderName}/000042_My Form",
            result);
    }

    [Fact]
    public void GetBasePath_PadsSubmissionIdToSixDigits()
    {
        var metadata = new ApplicationExportMetadataResponse
        {
            OrganisationName = "Org",
            Qan = "123",
            SubmissionId = 7,
            FormName = "Form"
        };

        var result = ApplicationExportPathBuilder.GetBasePath(metadata);

        Assert.Contains("/000007_Form", result);
    }
}