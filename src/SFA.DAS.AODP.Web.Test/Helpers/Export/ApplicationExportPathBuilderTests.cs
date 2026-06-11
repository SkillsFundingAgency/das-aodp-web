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
    public void GetZipFileName_TruncatesLongOrganisationNames()
    {
        var metadata = new ApplicationExportMetadataResponse
        {
            OrganisationName = "AldertonBridgewoodCavernlyDunmorexLoxburyMeridonHillcrossFarnleyEastwoodGroupX",
            Qan = "78945612",
            SubmissionId = 123456
        };

        var result = ApplicationExportPathBuilder.GetZipFileName(metadata);

        Assert.Equal(
            $"AldertonBridgewoodCavernlyDunmorexLoxburyMeridonHi_78945612_123456.zip",
            result);
    }
}