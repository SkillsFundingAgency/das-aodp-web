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

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class NewControllerTests
{
    private readonly Mock<ILogger<NewController>> _logger = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IUserHelperService> _userHelper = new();
    private readonly IOptions<AodpConfiguration> _options = Options.Create(new AodpConfiguration { FindRegulatedQualificationUrl = "http://find" });

    private NewController CreateController()
    {
        var controller = new NewController(_logger.Object, _options, _mediator.Object, _userHelper.Object);

        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        return controller;
    }

    private static BaseMediatrResponse<T> Success<T>(T value) where T : class, new()
    {
        return new BaseMediatrResponse<T> { Success = true, Value = value };
    }

    [Fact]
    public async Task Index_PageNumberZero_ReturnsViewWithDefaultModel()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new GetProcessStatusesQueryResponse { ProcessStatuses = new List<GetProcessStatusesQueryResponse.ProcessStatus>() }));

        var result = await controller.Index(null, pageNumber: 0);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NewQualificationsViewModel>(view.Model);
        Assert.Empty(model.NewQualifications);
    }

    [Fact]
    public async Task Index_PageNumberGreaterThanZero_FillsModelFromMediator()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new GetProcessStatusesQueryResponse { ProcessStatuses = new List<GetProcessStatusesQueryResponse.ProcessStatus>() }));

        var newQualsResp = new GetNewQualificationsQueryResponse
        {
            TotalRecords = 1,
            Skip = 0,
            Take = 10,
            Data = new List<NewQualification> { new() { Title = "T", Reference = "R", AwardingOrganisation = "AO", Status = "S", AgeGroup = "AG" } },
            Job = new Job { Name = "job", Status = "OK" }
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(newQualsResp));

        var result = await controller.Index(null, pageNumber: 1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NewQualificationsViewModel>(view.Model);
        Assert.Single(model.NewQualifications);
        Assert.Equal("T", model.NewQualifications.First().Title);
    }

    [Fact]
    public async Task Search_Post_RedirectsToIndexWithQuery()
    {
        var controller = CreateController();
        var vm = new NewQualificationsViewModel();
        vm.PaginationViewModel.RecordsPerPage = 10;
        vm.Filter.QualificationName = "name";
        vm.Filter.Organisation = "org";
        vm.Filter.QAN = "qan";
        vm.Filter.ProcessStatusIds = new List<Guid> { Guid.NewGuid() };

        var result = await controller.Search(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(NewController.Index), redirect.ActionName);
    }

    [Fact]
    public async Task Clear_ValidModelState_RedirectsToIndex()
    {
        var controller = CreateController();

        var result = await controller.Clear(10);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(NewController.Index), redirect.ActionName);
    }

    [Fact]
    public async Task Clear_InvalidModelState_ReturnsIndexView()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("k", "e");

        var result = await controller.Clear(10);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
    }

    [Fact]
    public async Task ChangePage_ValidModelState_RedirectsToIndex()
    {
        var controller = CreateController();

        var result = await controller.ChangePage(2, 10, "n", "o", "q");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(NewController.Index), redirect.ActionName);
    }

    [Fact]
    public async Task ChangePage_InvalidModelState_ReturnsIndexView()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("k", "e");

        var result = await controller.ChangePage(2, 10, "n", "o", "q");

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
    }

    [Fact]
    public async Task QualificationDetails_Get_WithEmptyRef_RedirectsToError()
    {
        var controller = CreateController();

        var result = await controller.QualificationDetails("");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task QualificationDetails_Post_WithNote_SendsAddDiscussionCommand_AndRedirects()
    {
        var controller = CreateController();

        var posted = new NewQualificationDetailsViewModel
        {
            Qual = new NewQualificationDetailsViewModel.Qualification { Qan = "Q" },
            AdditionalActions = new NewQualificationDetailsViewModel.AdditionalFormActions { Note = "note" }
        };

        _mediator.Setup(m => m.Send(It.IsAny<AddQualificationDiscussionHistoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new EmptyResponse()));

        var result = await controller.QualificationDetails(posted);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(NewController.QualificationDetails), redirect.ActionName);
        Assert.True(controller.TempData.ContainsKey(NewController.NewQualDataKeys.CommentSaved.ToString()));
    }

    [Fact]
    public async Task QualificationDetails_Post_WithNoNoteOrStatus_RedirectsToGet()
    {
        var controller = CreateController();

        var posted = new NewQualificationDetailsViewModel
        {
            Qual = new NewQualificationDetailsViewModel.Qualification { Qan = "Q" },
            AdditionalActions = new NewQualificationDetailsViewModel.AdditionalFormActions { Note = string.Empty, ProcessStatusId = null }
        };

        var result = await controller.QualificationDetails(posted);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(NewController.QualificationDetails), redirect.ActionName);
    }

    [Fact]
    public async Task QualificationDetails_Post_WithStatus_UserNotAllowed_RedirectsToGet()
    {
        var controller = CreateController();

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string>());

        var posted = new NewQualificationDetailsViewModel
        {
            Qual = new NewQualificationDetailsViewModel.Qualification { Qan = "Q" },
            AdditionalActions = new NewQualificationDetailsViewModel.AdditionalFormActions { ProcessStatusId = Guid.NewGuid() },
            ProcessStatuses = new List<NewQualificationDetailsViewModel.ProcessStatus> { new() { Id = Guid.NewGuid(), Name = "No Action Required" } },
            Stage = new NewQualificationDetailsViewModel.LifecycleStage { Name = "Draft" }
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new GetProcessStatusesQueryResponse { ProcessStatuses = new List<GetProcessStatusesQueryResponse.ProcessStatus> { new() { Id = posted.ProcessStatuses.First().Id, Name = "No Action Required" } } }));

        var result = await controller.QualificationDetails(posted);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(NewController.QualificationDetails), redirect.ActionName);
        _mediator.Verify(m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task QualificationDetailsTimeline_EmptyRef_RedirectsToError()
    {
        var controller = CreateController();

        var result = await controller.QualificationDetailsTimeline("");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task QualificationDetails_Get_ReturnsView_WithModel_And_DefaultBackData()
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
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Draft" },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "AO" },
            Version = 1,
            LastUpdatedDate = DateTime.UtcNow,
            UiLastUpdatedDate = DateTime.UtcNow,
            InsertedDate = DateTime.UtcNow
        };

        var procStatusesResponse = new GetProcessStatusesQueryResponse();
        procStatusesResponse.ProcessStatuses.Add(new GetProcessStatusesQueryResponse.ProcessStatus { Id = Guid.NewGuid(), Name = "Decision Required" });

        var feedbackResponse = new GetFeedbackForQualificationFundingByIdQueryResponse
        {
            QualificationFundedOffers = new List<GetFeedbackForQualificationFundingByIdQueryResponse.QualificationFunding>()
                {
                    new GetFeedbackForQualificationFundingByIdQueryResponse.QualificationFunding
                    {
                        Id = Guid.NewGuid(),
                        FundedOfferName = "Offer 1",
                        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                        EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(10))
                    }
                }
        };

        var appsResponse = new GetApplicationsByQanQueryResponse
        {
            Applications = new List<GetApplicationsByQanQueryResponse.Application>()
                {
                    new GetApplicationsByQanQueryResponse.Application
                    {
                        Id = Guid.NewGuid(),
                        ReferenceId = 1,
                        CreatedDate = DateTime.UtcNow.AddDays(-2),
                        SubmittedDate = DateTime.UtcNow.AddDays(-1),
                        Status = SFA.DAS.AODP.Models.Application.ApplicationStatus.Draft
                    }
                }
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
        var model = Assert.IsType<NewQualificationDetailsViewModel>(view.Model);
        Assert.Equal(qan, model.Qual.Qan);

        Assert.NotNull(model.ProcessStatuses);
        Assert.NotEmpty(model.ProcessStatuses);
        Assert.NotNull(model.Applications);
        Assert.NotEmpty(model.Applications);
        Assert.NotNull(model.FundingDetails);
        Assert.NotEmpty(model.FundingDetails);

        // Default back data set via ViewData
        Assert.Equal("Review", controller.ViewData["BackArea"]);
        Assert.Equal("New", controller.ViewData["BackController"]);
        Assert.Equal("Index", controller.ViewData["BackAction"]);
    }

    [Fact]
    public async Task QualificationDetails_Get_WithReturnToQualificationSearch_SetsBackToQualificationSearch()
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
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage { Id = Guid.NewGuid(), Name = "Draft" },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "AO" },
            Version = 1,
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
        var result = await controller.QualificationDetails(qan, "QualificationSearch");

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NewQualificationDetailsViewModel>(view.Model);
        Assert.Equal(qan, model.Qual.Qan);

        Assert.Equal("Review", controller.ViewData["BackArea"]);
        Assert.Equal("QualificationSearch", controller.ViewData["BackController"]);
        Assert.Equal("Index", controller.ViewData["BackAction"]);
    }

    [Fact]
    public async Task ExportData_WhenNoExports_RedirectsToError()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetNewQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationsExportResponse> { Success = false, ErrorMessage = "no" });

        var result = await controller.ExportData();

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task ExportData_WhenExports_ReturnsFile()
    {
        var controller = CreateController();

        var exportResp = new GetQualificationsExportResponse { QualificationExports = new List<QualificationExport> { new() { QualificationNumber = "Q" } } };

        _mediator.Setup(m => m.Send(It.IsAny<GetNewQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(exportResp));

        var result = await controller.ExportData();

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", file.ContentType);
        Assert.NotEmpty(file.FileContents);
    }
}
