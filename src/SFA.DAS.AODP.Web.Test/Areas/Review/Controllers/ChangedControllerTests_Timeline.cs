using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Application.Services;
using SFA.DAS.AODP.UnitTests.Helper;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class ChangedControllerTests_Timeline
{
    private readonly Mock<IMediator> _mediator;
    private readonly Mock<IUserHelperService> _userHelper;
    private readonly Mock<ILogger<ChangedController>> _logger;
    private readonly Mock<IQualificationTimelineHistoryBuilder> _timelineBuilder;
    private readonly ChangedController _controller;

    public ChangedControllerTests_Timeline()
    {
        _mediator = new Mock<IMediator>();
        _userHelper = new Mock<IUserHelperService>();
        _logger = new Mock<ILogger<ChangedController>>();
        _timelineBuilder = new Mock<IQualificationTimelineHistoryBuilder>();

        _controller = new ChangedController(
            _logger.Object,
            _mediator.Object,
            _userHelper.Object,
            _timelineBuilder.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _controller.TempData = new TempDataDictionary(
            httpContext,
            Mock.Of<ITempDataProvider>());

        _userHelper
            .Setup(u => u.GetUserRoles())
            .Returns(new List<string>());
    }

    [Fact]
    public async Task Timeline_EmptyReference_RedirectsToError()
    {
        var result = await _controller.QualificationDetailsTimeline("");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task Timeline_ReturnsViewModel_WithQan()
    {
        var response = new QualificationDiscussionHistoriesResponse
        {
            QualificationDiscussionHistories = new List<QualificationDiscussionHistory>()
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<GetQualificationTimelineQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<QualificationDiscussionHistoriesResponse>
            {
                Success = true,
                Value = response
            });

        var result = await _controller.QualificationDetailsTimeline("ABC123");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationDetailsTimelineViewModel>(view.Model);

        Assert.Equal("ABC123", model.Qan);
        Assert.Empty(model.QualificationDiscussionHistories);
    }

    [Fact]
    public async Task Timeline_ReturnsDiscussionHistoryEntries()
    {
        var response = new QualificationDiscussionHistoriesResponse
        {
            QualificationDiscussionHistories = new List<QualificationDiscussionHistory>
            {
                new QualificationDiscussionHistory
                {
                    Id = Guid.NewGuid(),
                    QualificationId = Guid.NewGuid(),
                    ActionTypeId = Guid.NewGuid(),
                    UserDisplayName = "System",
                    Notes = "Updated qualification",
                    Timestamp = DateTime.UtcNow,
                    Title = "Change",
                    ActionType = new Application.Queries.Qualifications.ActionType
                    {
                        Id = Guid.NewGuid(),
                        Description = "Change"
                    }
                }
            }
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<GetQualificationTimelineQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<QualificationDiscussionHistoriesResponse>
            {
                Success = true,
                Value = response
            });

        var result = await _controller.QualificationDetailsTimeline("ABC");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationDetailsTimelineViewModel>(view.Model);

        Assert.Single(model.QualificationDiscussionHistories);
        Assert.Equal("Change", model.QualificationDiscussionHistories[0].Title);
    }

    [Fact]
    public async Task Timeline_Failure_RedirectsToError()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<GetQualificationTimelineQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<QualificationDiscussionHistoriesResponse>
            {
                Success = false,
                ErrorMessage = "Boom"
            });

        var result = await _controller.QualificationDetailsTimeline("ABC");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task Timeline_Exception_RedirectsToError()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<GetQualificationTimelineQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await _controller.QualificationDetailsTimeline("ABC");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }
}
