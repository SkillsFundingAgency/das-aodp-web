using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Qualification;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Qualifications.Requests;
using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;
using AwardingOrganisation = SFA.DAS.AODP.Domain.Rollover.AwardingOrganisation;
using SFA.DAS.AODP.Web.Models.Session;
using System.Security.Claims;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class NewControllerTests
{
    private const string DefaultUserName = "TestUser";
    private const string DefaultQan = "61054902";
    private const string DefaultQualificationName = "Test Qualification";
    private const string DefaultOrganisationName = "Test Org";
    private const string DefaultComment = "This is a note";

    private const string DecisionRequiredStatus = "Decision Required";
    private const string NoActionRequiredStatus = "No Action Required";
    private const string NotAllowedStatus = "Not Allowed";

    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IUserHelperService> _userHelper = new();
    private readonly Mock<ILogger<NewController>> _logger = new();

    #region Helper methods
    private NewController CreateController()
    {
        var controller = new NewController(
            _logger.Object,
            _mediator.Object,
            _userHelper.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();

        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, DefaultUserName) },
                "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.TempData = new TempDataDictionary(
            httpContext,
            Mock.Of<ITempDataProvider>());

        return controller;
    }

    private static GetQualificationDetailsQueryResponse CreateQualificationDetailsResponse(
        string qan,
        string qualificationName = DefaultQualificationName,
        string organisationName = DefaultOrganisationName)
    {
        return new GetQualificationDetailsQueryResponse
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            VersionFieldChangesId = Guid.NewGuid(),
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = qan,
                QualificationName = qualificationName,
                Versions = new List<GetQualificationDetailsQueryResponse>()
            },
            ProcStatus = new ProcessStatus
            {
                Id = Guid.NewGuid(),
                Name = DecisionRequiredStatus
            },
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage
            {
                Id = Guid.NewGuid(),
                Name = "Draft"
            },
            Organisation = new AwardingOrganisation
            {
                Id = Guid.NewGuid(),
                NameOfqual = organisationName
            },
            Version = 1,
            LastUpdatedDate = DateTime.UtcNow,
            UiLastUpdatedDate = DateTime.UtcNow,
            InsertedDate = DateTime.UtcNow
        };
    }

    private static GetProcessStatusesQueryResponse CreateProcessStatusesResponse(params (Guid Id, string Name)[] statuses)
    {
        var response = new GetProcessStatusesQueryResponse();

        foreach (var status in statuses)
        {
            response.ProcessStatuses.Add(new ProcessStatus
            {
                Id = status.Id,
                Name = status.Name
            });
        }

        return response;
    }

    private void SetupQualificationDetailsDependencies(
        GetQualificationDetailsQueryResponse qualificationResponse,
        GetProcessStatusesQueryResponse processStatusesResponse)
    {
        var feedbackResponse = new GetFeedbackForQualificationFundingByIdQueryResponse
        {
            QualificationFundedOffers = new List<GetFeedbackForQualificationFundingByIdQueryResponse.QualificationFunding>()
        };

        var applicationsResponse = new GetApplicationsByQanQueryResponse
        {
            Applications = new List<GetApplicationsByQanQueryResponse.Application>()
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationDetailsQueryResponse>
            {
                Success = true,
                Value = qualificationResponse
            });

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = processStatusesResponse
            });

        _mediator.Setup(m => m.Send(It.IsAny<GetFeedbackForQualificationFundingByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse>
            {
                Success = true,
                Value = feedbackResponse
            });

        _mediator.Setup(m => m.Send(It.IsAny<GetApplicationsByQanQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetApplicationsByQanQueryResponse>
            {
                Success = true,
                Value = applicationsResponse
            });
    }

    #endregion

    #region QualificationDetails

    [Fact]
    public async Task QualificationDetails_Get_ReturnsRedirect_WhenReferenceMissing()
    {
        var controller = CreateController();

        var result = await controller.QualificationDetails("");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task QualificationDetails_Get_ReturnsView_WithModel()
    {
        var controller = CreateController();

        var qualificationResponse = CreateQualificationDetailsResponse(DefaultQan);
        var processStatusesResponse = CreateProcessStatusesResponse((Guid.NewGuid(), DecisionRequiredStatus));

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string>());
        SetupQualificationDetailsDependencies(qualificationResponse, processStatusesResponse);

        var result = await controller.QualificationDetails(DefaultQan);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NewQualificationDetailsViewModel>(view.Model);

        Assert.Equal(DefaultQan, model.Qual.Qan);
        Assert.NotEmpty(model.ProcessStatuses);
        Assert.NotNull(model.Applications);
        Assert.NotNull(model.FundingDetails);
    }

    [Fact]
    public async Task QualificationDetails_Post_AddsComment_WhenNoStatusProvided()
    {
        var controller = CreateController();

        var model = new NewQualificationDetailsViewModel
        {
            Qual = new NewQualificationDetailsViewModel.Qualification { Qan = DefaultQan },
            AdditionalActions = new NewQualificationDetailsViewModel.AdditionalFormActions
            {
                Note = DefaultComment,
                ProcessStatusId = null
            }
        };

        _mediator.Setup(m => m.Send(It.IsAny<AddQualificationDiscussionHistoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse>
            {
                Success = true,
                Value = new EmptyResponse()
            });

        var result = await controller.QualificationDetails(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(NewController.QualificationDetails), redirect.ActionName);
        Assert.True((bool)controller.TempData[NewController.NewQualDataKeys.CommentSaved.ToString()]!);
    }

    [Fact]
    public async Task QualificationDetails_Post_Redirects_WhenStatusNotAllowed()
    {
        var controller = CreateController();

        var processStatusId = Guid.NewGuid();

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string>());

        var processStatusesResponse = CreateProcessStatusesResponse((processStatusId, NotAllowedStatus));

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = processStatusesResponse
            });

        var model = new NewQualificationDetailsViewModel
        {
            Qual = new NewQualificationDetailsViewModel.Qualification { Qan = DefaultQan },
            AdditionalActions = new NewQualificationDetailsViewModel.AdditionalFormActions
            {
                ProcessStatusId = processStatusId
            }
        };

        var result = await controller.QualificationDetails(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(NewController.QualificationDetails), redirect.ActionName);

        _mediator.Verify(m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    #endregion

    #region Timeline

    [Fact]
    public async Task QualificationDetailsTimeline_ReturnsRedirect_WhenReferenceMissing()
    {
        var controller = CreateController();

        var result = await controller.QualificationDetailsTimeline("");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task QualificationDetailsTimeline_ReturnsView_WithModel()
    {
        var controller = CreateController();

        var timelineResponse = new QualificationDiscussionHistoriesResponse
        {
            QualificationDiscussionHistories = new List<QualificationDiscussionHistory>()
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetQualificationTimelineQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<QualificationDiscussionHistoriesResponse>
            {
                Success = true,
                Value = timelineResponse
            });

        var result = await controller.QualificationDetailsTimeline(DefaultQan);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationDetailsTimelineViewModel>(view.Model);

        Assert.Equal(DefaultQan, model.Qan);
    }
    [Fact]
    public async Task QualificationDetailsTimeline_WhenMediatorThrows_RedirectsToQualificationDetails()
    {
        var controller = CreateController();

        _mediator
            .Setup(m => m.Send(
                It.IsAny<GetQualificationTimelineQuery>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("test"));

        var result = await controller.QualificationDetailsTimeline(DefaultQan);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(NewController.QualificationDetails), redirect.ActionName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(DefaultQan, redirect.RouteValues["qualificationReference"]);
    }

    #endregion

    #region ExportData

    [Fact]
    public async Task ExportData_ReturnsFile_WhenExportsExist()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetNewQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationsExportResponse>
            {
                Success = true,
                Value = new GetQualificationsExportResponse
                {
                    QualificationExports = new List<QualificationExport>
                    {
                        new QualificationExport()
                    }
                }
            });

        var result = await controller.ExportData();

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", file.ContentType);
        Assert.NotEmpty(file.FileContents);
    }

    [Fact]
    public async Task ExportData_RedirectsToError_WhenNoExports()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetNewQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationsExportResponse>
            {
                Success = false
            });

        var result = await controller.ExportData();

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    #endregion

    #region Index

    [Fact]
    public async Task Index_ReturnsView_WithEmptyModel_WhenPageNumberZero()
    {
        var controller = CreateController();

        controller.HttpContext.Session.SetObject("NewQualificationFilters",
            new QualificationFilterSessionModel { PageNumber = 0, RecordsPerPage = 10 });

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = new GetProcessStatusesQueryResponse()
            });

        var result = await controller.Index(pageNumber: 0);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NewQualificationsViewModel>(view.Model);

        Assert.Empty(model.NewQualifications);
        Assert.NotNull(model.Filter);
        Assert.NotNull(model.ProcessStatuses);
    }

    [Fact]
    public async Task Index_ReturnsView_WithListOfNewQualifications()
    {
        var controller = CreateController();

        controller.HttpContext.Session.SetObject("NewQualificationFilters",
            new QualificationFilterSessionModel { PageNumber = 1, RecordsPerPage = 10 });

        var newData = new List<NewQualification>
        {
            new() { Title = "Math", AwardingOrganisation = "OrgA", Status = "Updated", AgeGroup = "AG" },
            new() { Title = "Science", AwardingOrganisation = "OrgB", Status = "Updated", AgeGroup = "AG" }
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetNewQualificationsQueryResponse>
            {
                Success = true,
                Value = new GetNewQualificationsQueryResponse
                {
                    Data = newData,
                    TotalRecords = 2,
                    Skip = 0,
                    Take = 10
                }
            });

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = new GetProcessStatusesQueryResponse()
            });

        var result = await controller.Index(pageNumber: 1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NewQualificationsViewModel>(view.Model);

        Assert.Equal(2, model.NewQualifications.Count);
        Assert.Equal("Math", model.NewQualifications[0].Title);
        Assert.Equal("OrgA", model.NewQualifications[0].AwardingOrganisation);
    }

    [Fact]
    public async Task Index_RedirectsToError_WhenMediatorFails()
    {
        var controller = CreateController();

        controller.HttpContext.Session.SetObject("NewQualificationFilters",
            new QualificationFilterSessionModel { PageNumber = 1 });

        _mediator.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetNewQualificationsQueryResponse>
            {
                Success = false
            });

        var result = await controller.Index(pageNumber: 1);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task Index_UpdatesSessionPageNumber()
    {
        var controller = CreateController();

        controller.HttpContext.Session.SetObject("NewQualificationFilters",
            new QualificationFilterSessionModel { PageNumber = 1 });

        _mediator.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetNewQualificationsQueryResponse>
            {
                Success = true,
                Value = new GetNewQualificationsQueryResponse { Data = new(), TotalRecords = 0 }
            });

        await controller.Index(pageNumber: 5);

        var updated = controller.HttpContext.Session.GetObject<QualificationFilterSessionModel>("NewQualificationFilters");
        Assert.Equal(5, updated.PageNumber);
    }

    [Fact]
    public async Task Index_InvalidPaging_ShowsNotification()
    {
        var controller = CreateController();

        controller.TempData[NewController.NewQualDataKeys.InvalidPageParams.ToString()] = true;

        controller.HttpContext.Session.SetObject("NewQualificationFilters",
            new QualificationFilterSessionModel
            {
                PageNumber = -1,
                RecordsPerPage = 999
            });

        var response = new BaseMediatrResponse<GetNewQualificationsQueryResponse>
        {
            Success = true,
            Value = new GetNewQualificationsQueryResponse
            {
                Data = new List<NewQualification>(),
                TotalRecords = 0,
                Skip = 0,
                Take = 10
            }
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await controller.Index(pageNumber: 1);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task Index_Exception_RedirectsToError()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        var result = await controller.Index(pageNumber: 1);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    #endregion

    #region Search

    [Fact]
    public async Task Search_WritesFiltersToSession_AndRedirectsToIndex()
    {
        var controller = CreateController();

        var viewModel = new NewQualificationsViewModel
        {
            Filter = new NewQualificationFilterViewModel
            {
                QualificationName = "Math",
                Organisation = "OrgA",
                QAN = "12345678",
                ProcessStatusIds = new List<Guid>(),
                AgeGroups = new List<AgeGroup>()
            },
            PaginationViewModel = new PaginationViewModel
            {
                RecordsPerPage = 20
            }
        };

        var result = await controller.Search(viewModel);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(NewController.Index), redirect.ActionName);

        var sessionModel = controller.HttpContext.Session
            .GetObject<QualificationFilterSessionModel>("NewQualificationFilters");

        Assert.NotNull(sessionModel);
        Assert.Equal("Math", sessionModel.QualificationName);
        Assert.Equal("OrgA", sessionModel.Organisation);
        Assert.Equal("12345678", sessionModel.QAN);
        Assert.Equal(20, sessionModel.RecordsPerPage);
    }

    [Fact]
    public async Task Search_ResetsPageNumberToOne()
    {
        var controller = CreateController();

        var vm = new NewQualificationsViewModel
        {
            Filter = new NewQualificationFilterViewModel(),
            PaginationViewModel = new PaginationViewModel { RecordsPerPage = 20 }
        };

        await controller.Search(vm);

        var session = controller.HttpContext.Session
            .GetObject<QualificationFilterSessionModel>("NewQualificationFilters");

        Assert.Equal(1, session.PageNumber);
    }

    [Fact]
    public async Task Search_MapsNullFieldsCorrectly()
    {
        var controller = CreateController();

        var vm = new NewQualificationsViewModel
        {
            Filter = new NewQualificationFilterViewModel(),
            PaginationViewModel = new PaginationViewModel { RecordsPerPage = 10 }
        };

        await controller.Search(vm);

        var session = controller.HttpContext.Session
            .GetObject<QualificationFilterSessionModel>("NewQualificationFilters");

        Assert.Equal("", session.QualificationName);
        Assert.Equal("", session.Organisation);
        Assert.Equal("", session.QAN);
    }

    [Fact]
    public async Task Search_Exception_ReturnsIndexView()
    {
        var controller = CreateController();

        var vm = new NewQualificationsViewModel
        {
            Filter = new NewQualificationFilterViewModel(),
            PaginationViewModel = new PaginationViewModel()
        };

        controller.HttpContext.Session = new TestSessionThrowing();

        var result = await controller.Search(vm);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
    }

    #endregion

    #region Clear

    [Fact]
    public async Task Clear_RemovesSession_AndRedirectsToIndex()
    {
        var controller = CreateController();

        controller.HttpContext.Session.SetObject(
            "NewQualificationFilters",
            new QualificationFilterSessionModel { PageNumber = 5 });

        var result = await controller.Clear(recordsPerPage: 20);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(NewController.Index), redirect.ActionName);
        Assert.Equal(0, redirect.RouteValues["pageNumber"]);
        Assert.Equal(20, redirect.RouteValues["recordsPerPage"]);

        var session = controller.HttpContext.Session.GetObject<QualificationFilterSessionModel>("NewQualificationFilters");
        Assert.Null(session);
    }

    [Fact]
    public async Task Clear_ReturnsIndexView_WhenModelStateInvalid()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("recordsPerPage", "invalid");

        var result = await controller.Clear(recordsPerPage: 10);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);

        controller.HttpContext.Session.SetObject("NewQualificationFilters",
            new QualificationFilterSessionModel { PageNumber = 3 });

        var session = controller.HttpContext.Session.GetObject<QualificationFilterSessionModel>("NewQualificationFilters");
        Assert.NotNull(session);
    }

    [Fact]
    public async Task Clear_Exception_ReturnsIndexView()
    {
        var controller = CreateController();

        controller.HttpContext.Session = new TestSessionThrowing();

        var result = await controller.Clear(recordsPerPage: 10);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
    }

    [Fact]
    public async Task Clear_ResetsPageNumberToZero()
    {
        var controller = CreateController();

        controller.HttpContext.Session.SetObject(
            "NewQualificationFilters",
            new QualificationFilterSessionModel { PageNumber = 7 });

        var result = await controller.Clear(recordsPerPage: 50);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(0, redirect.RouteValues["pageNumber"]);
        Assert.Equal(50, redirect.RouteValues["recordsPerPage"]);
    }

    #endregion
}
