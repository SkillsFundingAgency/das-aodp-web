using Moq;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Domain.Rollover;

public class RolloverSelectCandidatesTests
{
    [Fact]
    public void SetSelectCandidates_WithModel_MapsValuesToSessionAndReturnsSameSession()
    {
        // arrange
        var model = new RolloverSelectCandidatesViewModel
        {
            SelectedOption = SelectCandidatesForRollover.ImportAList,
            ReturnUrl = "/return"
        };

        var sessionMock = new Mock<Web.Areas.Review.Domain.Rollover.Rollover>();
        sessionMock.SetupAllProperties();
        sessionMock.Object.SelectCandidates = new RolloverSelectCandidates
        {
            SelectedOption = SelectCandidatesForRollover.GenerateAList,
            ReturnUrl = "/old"
        };

        var sut = new RolloverSelectCandidates();

        // act
        var returned = sut.SetSelectCandidates(sessionMock.Object, model);

        // assert
        Assert.Same(sessionMock.Object, returned);
        Assert.NotNull(returned.SelectCandidates);
        Assert.Equal(SelectCandidatesForRollover.ImportAList, returned.SelectCandidates.SelectedOption);
        Assert.Equal("/return", returned.SelectCandidates.ReturnUrl);
    }

    [Fact]
    public void SetSelectCandidates_WithNullPropertiesOnModel_SetsNullsOnSession()
    {
        // arrange
        var model = new RolloverSelectCandidatesViewModel
        {
            SelectedOption = null,
            ReturnUrl = null
        };

        var sessionMock = new Mock<Web.Areas.Review.Domain.Rollover.Rollover>();
        sessionMock.SetupAllProperties();
        sessionMock.Object.SelectCandidates = new RolloverSelectCandidates
        {
            SelectedOption = SelectCandidatesForRollover.GenerateAList,
            ReturnUrl = "/old"
        };

        var sut = new RolloverSelectCandidates();

        // act
        var returned = sut.SetSelectCandidates(sessionMock.Object, model);

        // assert
        Assert.Same(sessionMock.Object, returned);
        Assert.Null(returned.SelectCandidates.SelectedOption);
        Assert.Null(returned.SelectCandidates.ReturnUrl);
    }

    [Fact]
    public void SetSelectCandidates_NullSession_ThrowsNullReferenceException()
    {
        // arrange
        var model = new RolloverSelectCandidatesViewModel
        {
            SelectedOption = SelectCandidatesForRollover.ImportAList,
            ReturnUrl = "/return"
        };

        var sut = new RolloverSelectCandidates();

        // act & assert
        Assert.Throws<NullReferenceException>(() => sut.SetSelectCandidates(null!, model));
    }

    [Fact]
    public void SetSelectCandidates_SessionSelectCandidatesNull_ThrowsNullReferenceException()
    {
        // arrange
        var model = new RolloverSelectCandidatesViewModel
        {
            SelectedOption = SelectCandidatesForRollover.ImportAList,
            ReturnUrl = "/return"
        };

        var session = new Web.Areas.Review.Domain.Rollover.Rollover();
        var sut = new RolloverSelectCandidates();

        // act & assert
        Assert.Throws<NullReferenceException>(() => sut.SetSelectCandidates(session, model));
    }

    [Fact]
    public void SetSelectCandidates_NullModel_ThrowsNullReferenceException()
    {
        // arrange
        var sessionMock = new Mock<Web.Areas.Review.Domain.Rollover.Rollover>();
        sessionMock.SetupAllProperties();
        sessionMock.Object.SelectCandidates = new RolloverSelectCandidates();

        var sut = new RolloverSelectCandidates();

        // act & assert
        Assert.Throws<NullReferenceException>(() => sut.SetSelectCandidates(sessionMock.Object, null!));
    }
}