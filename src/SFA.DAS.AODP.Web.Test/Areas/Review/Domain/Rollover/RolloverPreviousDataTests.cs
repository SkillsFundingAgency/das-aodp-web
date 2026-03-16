using Moq;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Domain.Rollover;

public class RolloverPreviousDataTests
{
    [Fact]
    public void SetPreviousDataCandidate_MapsValuesToSessionAndReturnsSameSession()
    {
        // arrange
        var previousDataModel = new RolloverPreviousDataViewModel
        {
            CandidateCount = 7,
            SelectedOption = RolloverPreviousFileOption.RemovePrevious
        };

        var sessionMock = new Mock<Web.Areas.Review.Domain.Rollover.Rollover>();
        sessionMock.SetupAllProperties();
        sessionMock.Object.PreviousData = new RolloverPreviousData
        {
            CandidateCount = 0,
            SelectedOption = RolloverPreviousFileOption.ContinueProcessing
        };

        var sut = new RolloverPreviousData();

        // act
        var returned = sut.SetPreviousDataCandidate(sessionMock.Object, previousDataModel);

        // assert
        Assert.Same(sessionMock.Object, returned);
        Assert.NotNull(sessionMock.Object.PreviousData);
        Assert.Equal(7, sessionMock.Object.PreviousData.CandidateCount);
        Assert.Equal(RolloverPreviousFileOption.RemovePrevious, sessionMock.Object.PreviousData.SelectedOption);
    }

    [Fact]
    public void SetPreviousDataCandidate_WithNullPreviousDataOnSession_ThrowsNullReferenceException()
    {
        // arrange
        var session = new Web.Areas.Review.Domain.Rollover.Rollover();
        var previousDataModel = new RolloverPreviousDataViewModel
        {
            CandidateCount = 3,
            SelectedOption = RolloverPreviousFileOption.ContinueProcessing
        };

        var sut = new RolloverPreviousData();

        // act & assert
        Assert.Throws<NullReferenceException>(() => sut.SetPreviousDataCandidate(session, previousDataModel));
    }

    [Fact]
    public void SetPreviousDataCandidate_NullSession_ThrowsNullReferenceException()
    {
        // arrange
        var previousDataModel = new RolloverPreviousDataViewModel
        {
            CandidateCount = 2,
            SelectedOption = RolloverPreviousFileOption.ContinueProcessing
        };

        var sut = new RolloverPreviousData();

        // act & assert
        Assert.Throws<NullReferenceException>(() => sut.SetPreviousDataCandidate(null!, previousDataModel));
    }

    [Fact]
    public void SetPreviousDataCandidate_NullPreviousDataModel_ThrowsNullReferenceException()
    {
        // arrange
        var session = new Web.Areas.Review.Domain.Rollover.Rollover
        {
            PreviousData = new RolloverPreviousData()
        };

        var sut = new RolloverPreviousData();

        // act & assert
        Assert.Throws<NullReferenceException>(() => sut.SetPreviousDataCandidate(session, null!));
    }
}
