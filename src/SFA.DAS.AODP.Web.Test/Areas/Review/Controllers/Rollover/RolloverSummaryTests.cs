using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Models.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers.Rollover;

public class RolloverSummaryTests : RolloverControllerTestBase
{
    // -------------------------------------------------------
    // GET: /Review/Rollover/RolloverSummary
    // -------------------------------------------------------

    [Fact]
    public void RolloverSummary_Get_ReturnsViewWithModel()
    {
        var controller = CreateController(CreateEmptySession());

        var model = new RolloverSummaryViewModel();

        var result = controller.RolloverSummary(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(model, view.Model);
    }

    // -------------------------------------------------------
    // POST: /Review/Rollover/RolloverSummary
    // -------------------------------------------------------

    [Fact]
    public async Task RolloverSummary_Post_BuildsCommandCorrectly_AndSendsToMediator()
    {
        // Arrange
        var session = CreateEmptySession();

        var candidate = new FundingExtensionCandidate
        {
            Qan = "123",
            FundingStreamName = "FS1",
            RollOverStatus = "Extend",
            ExclusionReason = "None",
            ProposedFundingApprovalEndDate = DateTime.UtcNow, // REQUIRED
            Comments = "Test comment"
        };

        var rollover = new AODP.Domain.Rollover.Rollover
        {
            RolloverFundingExtensionCandidates = [candidate]
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(rollover));

        var controller = CreateController(session);

        MediatorMock
            .Setup(m => m.Send(It.IsAny<SubmitRolloverExtensionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<SubmitRolloverExtensionCommandResponse>
            {
                Success = true,
                Value = new SubmitRolloverExtensionCommandResponse()
            });

        // Act
        var result = await controller.RolloverSummary();

        // Assert mediator call
        MediatorMock.Verify(m =>
            m.Send(It.Is<SubmitRolloverExtensionCommand>(cmd =>
                cmd.Items.Count == 1 &&
                cmd.Items[0].Qan == "123" &&
                cmd.Items[0].FundingStreamName == "FS1" &&
                cmd.Items[0].RolloverStatus == "Extend" &&
                cmd.Items[0].ProposedFundingApprovalEndDate != null
            ),
            It.IsAny<CancellationToken>()),
            Times.Once);

        // Assert redirect
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.RolloverSubmitted), redirect.ActionName);
    }

    [Fact]
    public async Task RolloverSummary_Post_ClearsSession_AfterSubmit()
    {
        // Arrange
        var session = CreateEmptySession();

        var rollover = new AODP.Domain.Rollover.Rollover
        {
            RolloverFundingExtensionCandidates =
            [
                new()
                {
                    Qan = "123",
                    FundingStreamName = "FS1",
                    RollOverStatus = "Extend",
                    ProposedFundingApprovalEndDate = DateTime.UtcNow // REQUIRED
                }
            ]
        };

        session.SetString("RolloverSession", JsonConvert.SerializeObject(rollover));

        var controller = CreateController(session);

        MediatorMock
            .Setup(m => m.Send(It.IsAny<SubmitRolloverExtensionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<SubmitRolloverExtensionCommandResponse>
            {
                Success = true,
                Value = new SubmitRolloverExtensionCommandResponse()
            });

        // Act
        var result = await controller.RolloverSummary();

        // Assert session cleared
        Assert.False(session.TryGetValue("RolloverSession", out _));

        // Assert redirect
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.RolloverSubmitted), redirect.ActionName);
    }

    // -------------------------------------------------------
    // GET: /Review/Rollover/RolloverSubmitted
    // -------------------------------------------------------

    [Fact]
    public async Task RolloverSubmitted_Get_ReturnsView()
    {
        var controller = CreateController(CreateEmptySession());

        var result = await controller.RolloverSubmitted();

        Assert.IsType<ViewResult>(result);
    }
}