using Moq;
using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Domain.Rollover;

public class RolloverStartTests
{
    [Fact]
    public void SetStart_WithModel_SetsSelectedProcessOnSessionAndReturnsSameSession()
    {
        // arrange
        var model = new RolloverStartViewModel
        {
            SelectedProcess = RolloverProcess.FinalUpload
        };

        var session = new AODP.Domain.Rollover.Rollover
        {
            Start = new RolloverStart()
        };

        var sut = new RolloverStart();

        // act
        var returned = sut.SetStart(session, model.SelectedProcess);

        // assert
        Assert.Same(session, returned);
        Assert.Equal(RolloverProcess.FinalUpload, session.Start!.SelectedProcess);
    }

    [Fact]
    public void SetStart_WithNullSelectedProcess_SetsNullOnSession()
    {
        // arrange
        var model = new RolloverStartViewModel
        {
            SelectedProcess = null
        };

        var session = new AODP.Domain.Rollover.Rollover
        {
            Start = new RolloverStart { SelectedProcess = RolloverProcess.InitialSelection }
        };

        var sut = new RolloverStart();

        // act
        var returned = sut.SetStart(session, model.SelectedProcess);

        // assert
        Assert.Same(session, returned);
        Assert.Null(session.Start!.SelectedProcess);
    }

    [Fact]
    public void SetStart_NullSession_ThrowsNullReferenceException()
    {
        // arrange
        var model = new RolloverStartViewModel
        {
            SelectedProcess = RolloverProcess.InitialSelection
        };

        var sut = new RolloverStart();

        // act & assert
        Assert.Throws<NullReferenceException>(() => sut.SetStart(null!, model.SelectedProcess));
    }

    [Fact]
    public void SetStart_SessionStartNull_ThrowsNullReferenceException()
    {
        // arrange
        var model = new RolloverStartViewModel
        {
            SelectedProcess = RolloverProcess.InitialSelection
        };

        var session = new AODP.Domain.Rollover.Rollover
        {
            Start = null
        };

        var sut = new RolloverStart();

        // act & assert
        Assert.Throws<NullReferenceException>(() => sut.SetStart(session, model.SelectedProcess));
    }

    [Fact]
    public void SetStart_NullModel_ThrowsNullReferenceException()
    {
        // demonstrate Moq usage while keeping test deterministic
        var sessionMock = new Mock<AODP.Domain.Rollover.Rollover>();
        sessionMock.SetupAllProperties();
        sessionMock.Object.Start = null;

        var sut = new RolloverStart();

        // act & assert
        Assert.Throws<NullReferenceException>(() => sut.SetStart(sessionMock.Object, null!));
    }
}