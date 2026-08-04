using Moq;
using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Domain.Rollover;

public class RolloverImportStatusTests
{
    [Fact]
    public void SetImportStatus_WithModel_MapsValuesToSessionAndReturnsSameSession()
    {
        // arrange
        var importer = new RolloverImportStatus();
        var session = new AODP.Domain.Rollover.Rollover();
        var model = new RolloverImportStatusViewModel
        {
            RegulatedQualificationsLastImported = new DateTime(2025, 2, 2),
            FundedQualificationsLastImported = new DateTime(2025, 2, 3),
            DefundingListLastImported = new DateTime(2025, 2, 4),
            PldnsListLastImported = new DateTime(2025, 2, 5)
        };

        // act
        var returned = importer.SetImportStatus(session, model.RegulatedQualificationsLastImported,
            model.FundedQualificationsLastImported, model.DefundingListLastImported, model.PldnsListLastImported);

        // assert
        Assert.Same(session, returned);
        Assert.NotNull(session.ImportStatus);
        Assert.Equal(model.RegulatedQualificationsLastImported, session.ImportStatus.RegulatedQualificationsLastImported);
        Assert.Equal(model.FundedQualificationsLastImported, session.ImportStatus.FundedQualificationsLastImported);
        Assert.Equal(model.DefundingListLastImported, session.ImportStatus.DefundingListLastImported);
        Assert.Equal(model.PldnsListLastImported, session.ImportStatus.PldnsListLastImported);
    }

    [Fact]
    public void SetImportStatus_WithModelPropertiesNull_SetsSessionImportStatusWithNulls()
    {
        // arrange
        var importer = new RolloverImportStatus();

        var sessionMock = new Mock<AODP.Domain.Rollover.Rollover>();
        sessionMock.SetupAllProperties(); 
        var session = sessionMock.Object;

        var model = new RolloverImportStatusViewModel(); 

        // act
        var returned = importer.SetImportStatus(session, model.RegulatedQualificationsLastImported,
            model.FundedQualificationsLastImported, model.DefundingListLastImported, model.PldnsListLastImported);

        // assert
        Assert.Same(session, returned);
        Assert.NotNull(session.ImportStatus); 
        Assert.Null(session.ImportStatus.RegulatedQualificationsLastImported);
        Assert.Null(session.ImportStatus.FundedQualificationsLastImported);
        Assert.Null(session.ImportStatus.DefundingListLastImported);
        Assert.Null(session.ImportStatus.PldnsListLastImported);
    }
}
