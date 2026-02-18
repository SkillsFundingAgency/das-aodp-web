using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Qualification;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Security.Claims;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class ChangedControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IUserHelperService> _userHelper = new();
    private readonly Mock<ILogger<ChangedController>> _logger = new();
    private readonly IOptions<AodpConfiguration> _options = Options.Create(new AodpConfiguration());
    private ChangedController CreateController()
    {
        var controller = new ChangedController(_logger.Object, _options, _mediator.Object, _userHelper.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.Name, "TestUser") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        return controller;
    }

    [Fact]
    public async Task QualificationDetails_Get_ReturnsRedirect_WhenQualificationReferenceNull()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.QualificationDetails((string?)null);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task QualificationDetails_Get_ReturnsView_WithModel()
    {
        // Arrange
        var controller = CreateController();
        var qan = "61054902";

        var qualificationResponse = new GetQualificationDetailsQueryResponse
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            VersionFieldChangesId = Guid.NewGuid(),
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = qan,
                QualificationName = "Test Qualification",
                Versions = new List<GetQualificationDetailsQueryResponse>()
            },
            ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Decision Required" },
            Version = 1,
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Draft" },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Test Org" },
            LastUpdatedDate = DateTime.UtcNow,
            UiLastUpdatedDate = DateTime.UtcNow,
            InsertedDate = DateTime.UtcNow
        };

        var procStatusesResponse = new GetProcessStatusesQueryResponse();
        procStatusesResponse.ProcessStatuses.Add(new GetProcessStatusesQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Decision Required" });

        var feedbackResponse = new GetFeedbackForQualificationFundingByIdQueryResponse
        {
            QualificationFundedOffers = new List<GetFeedbackForQualificationFundingByIdQueryResponse.QualificationFunding>()
        };

        var appsResponse = new GetApplicationsByQanQueryResponse
        {
            Applications = new List<GetApplicationsByQanQueryResponse.Application>()
        };

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string>());

        _mediator.Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationDetailsQueryResponse> { Success = true, Value = qualificationResponse });

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse> { Success = true, Value = procStatusesResponse });

        _mediator.Setup(m => m.Send(It.IsAny<GetFeedbackForQualificationFundingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse> { Success = true, Value = feedbackResponse });

        _mediator.Setup(m => m.Send(It.IsAny<GetApplicationsByQanQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetApplicationsByQanQueryResponse> { Success = true, Value = appsResponse });

        // Act
        var result = await controller.QualificationDetails(qan);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ChangedQualificationDetailsViewModel>(view.Model);
        Assert.Equal(qan, model.Qual.Qan);
        Assert.NotNull(model.ProcessStatuses);
        Assert.NotNull(model.Applications);
    }

    [Fact]
    public async Task QualificationDetails_Post_AddsComment_WhenNoProcStatusAndNote()
    {
        // Arrange
        var controller = CreateController();

        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new ChangedQualificationDetailsViewModel.Qualification { Qan = "61054902" },
            AdditionalActions = new ChangedQualificationDetailsViewModel.AdditionalFormActions
            {
                Note = "This is a note",
                ProcessStatusId = null
            }
        };

        _mediator.Setup(m => m.Send(It.IsAny<AddQualificationDiscussionHistoryCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new BaseMediatrResponse<EmptyResponse> { Success = true, Value = new EmptyResponse() }));

        // Act
        var result = await controller.QualificationDetails(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ChangedController.QualificationDetails), redirect.ActionName);
        Assert.Equal(model.Qual.Qan, redirect.RouteValues["qualificationReference"]?.ToString());
    }

    [Fact]
    public async Task QualificationDetails_Post_WithProcessStatus_NotAllowedUser_RedirectsToQualificationDetails()
    {
        // Arrange
        var controller = CreateController();

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string> { "some_other_role" });

        // GetProcessStatuses will be called to populate model.ProcessStatuses
        var processStatusId = Guid.NewGuid();
        var procStatusesResponse = new GetProcessStatusesQueryResponse();
        procStatusesResponse.ProcessStatuses.Add(new GetProcessStatusesQueryResponse.ProcessStatus { Id = processStatusId, Name = "NotAllowed" });

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse> { Success = true, Value = procStatusesResponse });

        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new ChangedQualificationDetailsViewModel.Qualification { Qan = "61054902" },
            AdditionalActions = new ChangedQualificationDetailsViewModel.AdditionalFormActions
            {
                Note = "",
                ProcessStatusId = processStatusId
            }
        };

        // Act
        var result = await controller.QualificationDetails(model);

        // Assert 
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ChangedController.QualificationDetails), redirect.ActionName);
        Assert.Equal(model.Qual.Qan, redirect.RouteValues["qualificationReference"]?.ToString());
    }

    [Fact]
    public async Task QualificationDetails_Post_WithProcessStatus_AllowedUser_SendsUpdateCommand()
    {
        // Arrange
        var controller = CreateController();

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string> { "qfau_user_approver" });

        var processStatusId = Guid.NewGuid();
        var procStatusesResponse = new GetProcessStatusesQueryResponse();
        procStatusesResponse.ProcessStatuses.Add(new GetProcessStatusesQueryResponse.ProcessStatus { Id = processStatusId, Name = "Decision Required" });

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse> { Success = true, Value = procStatusesResponse });

        _mediator.Setup(m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse> { Success = true, Value = new EmptyResponse() });

        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new ChangedQualificationDetailsViewModel.Qualification { Qan = "61054902" },
            Version = 1,
            AdditionalActions = new ChangedQualificationDetailsViewModel.AdditionalFormActions
            {
                Note = "note",
                ProcessStatusId = processStatusId
            }
        };

        // Act
        var result = await controller.QualificationDetails(model);

        // Assert
        _mediator.Verify(m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ChangedController.QualificationDetails), redirect.ActionName);
        Assert.Equal(model.Qual.Qan, redirect.RouteValues["qualificationReference"]);
    }
}