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
using SFA.DAS.AODP.Application.Services;
using SFA.DAS.AODP.Domain.Qualifications.Requests;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;
using SFA.DAS.AODP.Web.Models.Session;
using System.Security.Claims;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class ChangedControllerTests
{
    private const string DefaultUserName = "TestUser";
    private const string DefaultQan = "61054902";
    private const string DefaultQualificationName = "Test Qualification";
    private const string DefaultOrganisationName = "Test Org";
    private const string DefaultComment = "This is a note";
    private const string DefaultFindQualificationUrl = "https://find-a-qualification.services.ofqual.gov.uk/qualifications/";

    private const string DecisionRequiredStatus = "Decision Required";
    private const string NoActionRequiredStatus = "No Action Required";
    private const string OnHoldStatus = "On Hold";
    private const string DisallowedStatus = "NotAllowed";

    private const string SearchName = "Qual";
    private const string SearchOrganisation = "Org";
    private const string SearchQan = "12345678";

    private const int DefaultPageNumber = 2;
    private const int DefaultRecordsPerPage = 20;
    private const int ResetPageNumber = 1;
    private const int ClearedPageNumber = 0;
    private const int ChangedPageNumber = 3;
    private const int VersionOne = 1;
    private const int VersionTwo = 2;

    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IUserHelperService> _userHelper = new();
    private readonly Mock<ILogger<ChangedController>> _logger = new();
    private readonly Mock<IQualificationTimelineHistoryBuilder> _timelineBuilder = new();

    private readonly IOptions<AodpConfiguration> _options =
        Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = DefaultFindQualificationUrl
        });

    public ChangedControllerTests()
    {
        _userHelper
            .Setup(u => u.GetUserRoles())
            .Returns(new List<string>());

        _mediator
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = new GetProcessStatusesQueryResponse
                {
                    ProcessStatuses =
                    {
                    new() { Id = Guid.NewGuid(), Name = "Decision Required" },
                    new() { Id = Guid.NewGuid(), Name = "No Action Required" }
                    }
                }
            });
    }

    #region Helper methods

    private ChangedController CreateController()
    {
        var controller = new ChangedController(
            _logger.Object,
            _mediator.Object,
            _userHelper.Object,
            _timelineBuilder.Object);

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
        int version = VersionOne,
        string qualificationName = DefaultQualificationName,
        string organisationName = DefaultOrganisationName,
        string? changedFieldNames = null)
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
            Version = version,
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage
            {
                Id = Guid.NewGuid(),
                Name = "Draft"
            },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation
            {
                Id = Guid.NewGuid(),
                NameOfqual = organisationName
            },
            VersionFieldChanges = changedFieldNames,
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

    [Fact]
    public async Task QualificationDetails_Get_ReturnsRedirect_WhenQualificationReferenceNull()
    {
        var controller = CreateController();

        var result = await controller.QualificationDetails(qualificationReference: null!);

        var redirect = Assert.IsType<RedirectResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal("/Home/Error", redirect.Url);
        });
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
        var model = Assert.IsType<ChangedQualificationDetailsViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.Equal(DefaultQan, model.Qual.Qan);
            Assert.NotNull(model.ProcessStatuses);
            Assert.NotNull(model.Applications);
            Assert.Equal(DefaultQualificationName, model.Qual.QualificationName);
        });
    }

    [Fact]
    public async Task QualificationDetails_Post_AddsComment_WhenNoProcStatusAndNote()
    {
        var controller = CreateController();

        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new Qualification { Qan = DefaultQan },
            AdditionalActions = new AdditionalFormActions
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

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(ChangedController.QualificationDetails), redirect.ActionName);
            Assert.Equal(DefaultQan, redirect.RouteValues!["qualificationReference"]?.ToString());
            Assert.True((bool)controller.TempData[ChangedController.NewQualDataKeys.CommentSaved.ToString()]!);
        });

        _mediator.Verify(m => m.Send(
            It.Is<AddQualificationDiscussionHistoryCommand>(cmd =>
                cmd.QualificationReference == DefaultQan &&
                cmd.Notes == DefaultComment &&
                cmd.UserDisplayName == DefaultUserName),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task QualificationDetails_Post_Redirects_WhenNoProcStatusAndNoNote()
    {
        var controller = CreateController();

        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new Qualification { Qan = DefaultQan },
            AdditionalActions = new AdditionalFormActions
            {
                Note = string.Empty,
                ProcessStatusId = null
            }
        };

        var result = await controller.QualificationDetails(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(ChangedController.QualificationDetails), redirect.ActionName);
            Assert.Equal(DefaultQan, redirect.RouteValues!["qualificationReference"]?.ToString());
        });

        _mediator.Verify(m => m.Send(It.IsAny<AddQualificationDiscussionHistoryCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _mediator.Verify(m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task QualificationDetails_Post_WithProcessStatus_NotAllowedUser_RedirectsToQualificationDetails()
    {
        var controller = CreateController();
        var processStatusId = Guid.NewGuid();

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string> { "some_other_role" });

        var processStatusesResponse = CreateProcessStatusesResponse((processStatusId, DisallowedStatus));

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = processStatusesResponse
            });

        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new Qualification { Qan = DefaultQan },
            AdditionalActions = new AdditionalFormActions
            {
                Note = string.Empty,
                ProcessStatusId = processStatusId
            }
        };

        var result = await controller.QualificationDetails(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(ChangedController.QualificationDetails), redirect.ActionName);
            Assert.Equal(DefaultQan, redirect.RouteValues!["qualificationReference"]?.ToString());
        });

        _mediator.Verify(m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task QualificationDetails_Post_WithProcessStatus_AllowedUser_SendsUpdateCommand()
    {
        var controller = CreateController();
        var processStatusId = Guid.NewGuid();

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string> { "qfau_user_approver" });

        var processStatusesResponse = CreateProcessStatusesResponse((processStatusId, DecisionRequiredStatus));

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = processStatusesResponse
            });

        _mediator.Setup(m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<EmptyResponse>
            {
                Success = true,
                Value = new EmptyResponse()
            });

        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new Qualification { Qan = DefaultQan },
            Version = VersionOne,
            AdditionalActions = new AdditionalFormActions
            {
                Note = DefaultComment,
                ProcessStatusId = processStatusId
            }
        };

        var result = await controller.QualificationDetails(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(ChangedController.QualificationDetails), redirect.ActionName);
            Assert.Equal(DefaultQan, redirect.RouteValues!["qualificationReference"]);
        });

        _mediator.Verify(m => m.Send(
            It.Is<UpdateQualificationStatusCommand>(cmd =>
                cmd.QualificationReference == DefaultQan &&
                cmd.ProcessStatusId == processStatusId &&
                cmd.Notes == DefaultComment &&
                cmd.Version == VersionOne &&
                cmd.UserDisplayName == DefaultUserName),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task QualificationDetails_Post_WhenExceptionThrown_RedirectsToQualificationDetails()
    {
        var controller = CreateController();
        var processStatusId = Guid.NewGuid();

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string> { "qfau_user_approver" });

        var processStatusesResponse = CreateProcessStatusesResponse((processStatusId, DecisionRequiredStatus));

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = processStatusesResponse
            });

        _mediator.Setup(m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new Qualification { Qan = DefaultQan },
            Version = VersionOne,
            AdditionalActions = new AdditionalFormActions
            {
                Note = DefaultComment,
                ProcessStatusId = processStatusId
            }
        };

        var result = await controller.QualificationDetails(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(ChangedController.QualificationDetails), redirect.ActionName);
            Assert.Equal(DefaultQan, redirect.RouteValues!["qualificationReference"]?.ToString());
        });
    }

    [Fact]
    public async Task GetProcessStatuses_ReturnsOnlyReviewerAllowedStatuses_WhenUserIsNotApprover()
    {
        var controller = CreateController();

        var allowedStatusId = Guid.NewGuid();
        var secondAllowedStatusId = Guid.NewGuid();
        var disallowedStatusId = Guid.NewGuid();

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string>());

        var processStatusesResponse = CreateProcessStatusesResponse(
            (allowedStatusId, DecisionRequiredStatus),
            (secondAllowedStatusId, NoActionRequiredStatus),
            (disallowedStatusId, OnHoldStatus));

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = processStatusesResponse
            });

        var result = await controller.GetProcessStatuses();

        Assert.Multiple(() =>
        {
            Assert.Equal(2, result.Count);
            Assert.Contains(result, s => s.Id == allowedStatusId && s.Name == DecisionRequiredStatus);
            Assert.Contains(result, s => s.Id == secondAllowedStatusId && s.Name == NoActionRequiredStatus);
            Assert.DoesNotContain(result, s => s.Id == disallowedStatusId && s.Name == OnHoldStatus);
        });
    }

    [Fact]
    public async Task GetProcessStatuses_ReturnsAllStatuses_WhenUserIsApprover()
    {
        var controller = CreateController();

        var allowedStatusId = Guid.NewGuid();
        var onHoldStatusId = Guid.NewGuid();

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string> { "qfau_user_approver" });

        var processStatusesResponse = CreateProcessStatusesResponse(
            (allowedStatusId, DecisionRequiredStatus),
            (onHoldStatusId, OnHoldStatus));

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = processStatusesResponse
            });

        var result = await controller.GetProcessStatuses();

        Assert.Multiple(() =>
        {
            Assert.Equal(2, result.Count);
            Assert.Contains(result, s => s.Id == allowedStatusId && s.Name == DecisionRequiredStatus);
            Assert.Contains(result, s => s.Id == onHoldStatusId && s.Name == OnHoldStatus);
        });
    }

    [Fact]
    public async Task ExportData_ReturnsFileContentResult_WhenExportExists()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetChangedQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
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

        Assert.Multiple(() =>
        {
            Assert.Equal("text/csv", file.ContentType);
            Assert.NotNull(file.FileContents);
            Assert.NotEmpty(file.FileContents);
            Assert.EndsWith("-ChangedQualificationsExport.csv", file.FileDownloadName);
        });
    }

    [Fact]
    public async Task ExportData_RedirectsToHomeError_WhenExportMissing()
    {
        var controller = CreateController();

        _mediator
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BaseMediatrResponse<GetQualificationsExportResponse>?)null!);

        var result = await controller.ExportData();

        var redirect = Assert.IsType<RedirectResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal("/Home/Error", redirect.Url);
        });
    }

    [Fact]
    public async Task ExportData_RedirectsToHomeError_WhenExceptionThrown()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetChangedQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await controller.ExportData();

        var redirect = Assert.IsType<RedirectResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal("/Home/Error", redirect.Url);
        });
    }

    [Fact]
    public async Task QualificationDetailsTimeline_ReturnsRedirect_WhenQualificationReferenceMissing()
    {
        var controller = CreateController();

        var result = await controller.QualificationDetailsTimeline(qualificationReference: null!);

        var redirect = Assert.IsType<RedirectResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal("/Home/Error", redirect.Url);
        });
    }

    [Fact]
    public async Task QualificationDetailsTimeline_ReturnsView_WithTimelineModel()
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

        _mediator.Verify(m => m.Send(
            It.Is<GetQualificationTimelineQuery>(q => q.QualificationReference == DefaultQan),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task QualificationDetailsTimeline_ReturnsRedirect_WhenExceptionThrown()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetQualificationTimelineQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await controller.QualificationDetailsTimeline(DefaultQan);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    #region Index

    [Fact]
    public async Task Index_ReturnsViewResult_Empty()
    {
        var controller = CreateController();

        controller.HttpContext.Session.SetObject("ChangedQualificationFilters",
            new QualificationFilterSessionModel
            {
                PageNumber = 1,
                RecordsPerPage = 10
            });

        var response = new BaseMediatrResponse<GetChangedQualificationsQueryResponse>
        {
            Success = true,
            Value = new GetChangedQualificationsQueryResponse
            {
                Data = new List<ChangedQualification>(),
                TotalRecords = 0,
                Skip = 0,
                Take = 10
            }
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await controller.Index(pageNumber: 1);

        Assert.IsType<ViewResult>(result);
    }


    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfNewQualifications()
    {
        var controller = CreateController();

        controller.HttpContext.Session.SetObject("ChangedQualificationFilters",
            new QualificationFilterSessionModel
            {
                PageNumber = 1,
                RecordsPerPage = 10
            });

        var items = new List<ChangedQualification>
    {
        new ChangedQualification
        {
            QualificationId = Guid.NewGuid(),
            Subject = "Math",
            AwardingOrganisation = "OrgA",
            Status = ProcessStatusLookup.DecisionRequired.Name
        },
        new ChangedQualification
        {
            QualificationId = Guid.NewGuid(),
            Subject = "English",
            AwardingOrganisation = "OrgB",
            Status = ProcessStatusLookup.DecisionRequired.Name
        }
    };

        var response = new BaseMediatrResponse<GetChangedQualificationsQueryResponse>
        {
            Success = true,
            Value = new GetChangedQualificationsQueryResponse
            {
                Data = items,
                TotalRecords = 2,
                Skip = 0,
                Take = 10
            }
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await controller.Index(pageNumber: 1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ChangedQualificationsViewModel>(view.Model);

        Assert.Equal(2, model.ChangedQualifications.Count);
        Assert.Equal("Math", model.ChangedQualifications[0].Subject);
        Assert.Equal("OrgA", model.ChangedQualifications[0].AwardingOrganisationName);
        Assert.Equal(ProcessStatusLookup.DecisionRequired.Name, model.ChangedQualifications[0].CurrentProcessStatus.Name);
    }

    [Fact]
    public async Task Index_ReturnsNotFound_WhenQueryFails()
    {
        var controller = CreateController();

        var queryResponse = new BaseMediatrResponse<GetChangedQualificationsQueryResponse>
        {
            Success = false
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(queryResponse);

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
                 {
                     Success = true,
                     Value = new GetProcessStatusesQueryResponse()
                 });

        var sessionModel = new QualificationFilterSessionModel
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        controller.HttpContext.Session.SetObject("ChangedQualificationFilters", sessionModel);

        var result = await controller.Index(pageNumber: 1);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task Index_StoresUpdatedSessionModel()
    {
        var controller = CreateController();

        var sessionModel = new QualificationFilterSessionModel
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        controller.HttpContext.Session.SetObject("ChangedQualificationFilters", sessionModel);

        _mediator
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetChangedQualificationsQueryResponse>
            {
                Success = true,
                Value = new GetChangedQualificationsQueryResponse { Data = new(), TotalRecords = 0 }
            });

        await controller.Index(pageNumber: 5);

        var updated = controller.HttpContext.Session.GetObject<QualificationFilterSessionModel>("ChangedQualificationFilters");
        Assert.Equal(5, updated.PageNumber);
    }

    [Fact]
    public async Task Index_InvalidPaging_ShowsNotification()
    {
        var controller = CreateController();

        var sessionModel = new QualificationFilterSessionModel
        {
            PageNumber = -1,
            RecordsPerPage = 999
        };

        controller.HttpContext.Session.SetObject("ChangedQualificationFilters", sessionModel);

        controller.TempData[ChangedController.NewQualDataKeys.InvalidPageParams.ToString()] = true;

        _mediator
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetChangedQualificationsQueryResponse>
            {
                Success = true,
                Value = new GetChangedQualificationsQueryResponse { Data = new(), TotalRecords = 0 }
            });

        var result = await controller.Index(pageNumber: -1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(controller.TempData[ChangedController.NewQualDataKeys.InvalidPageParams.ToString()]);
    }

    [Fact]
    public async Task Index_Exception_RedirectsToError()
    {
        var controller = CreateController();

        _mediator
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        var result = await controller.Index(pageNumber: 1);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    #endregion

    #region Clear

    [Fact]
    public async Task Clear_Empty_RedirectsToIndexWithDefaults()
    {
        var controller = CreateController();

        var result = await controller.Clear();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ChangedController.Index), redirect.ActionName);
        Assert.Equal(0, redirect.RouteValues["pageNumber"]);
        Assert.Equal(10, redirect.RouteValues["recordsPerPage"]);
    }

    [Fact]
    public async Task Clear_RemovesSessionKey()
    {
        var controller = CreateController();

        controller.HttpContext.Session.SetObject(
            "ChangedQualificationFilters",
            new QualificationFilterSessionModel());

        await controller.Clear();

        Assert.Null(controller.HttpContext.Session
            .GetObject<QualificationFilterSessionModel>("ChangedQualificationFilters"));
    }

    [Fact]
    public async Task Clear_InvalidModelState_ReturnsIndexView()
    {
        var controller = CreateController();

        controller.ModelState.AddModelError("x", "y");

        var result = await controller.Clear();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
    }

    [Fact]
    public async Task Clear_Exception_ReturnsIndexView()
    {
        var controller = CreateController();

        controller.HttpContext.Session = new TestSessionThrowing();

        var result = await controller.Clear();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
    }

    #endregion

    #region Search
    [Fact]
    public async Task Search_WritesFiltersToSession_AndRedirectsToIndex()
    {
        var controller = CreateController();

        var viewModel = new ChangedQualificationsViewModel
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
        Assert.Equal(nameof(ChangedController.Index), redirect.ActionName);

        var sessionModel = controller.HttpContext.Session
            .GetObject<QualificationFilterSessionModel>("ChangedQualificationFilters");

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

        var vm = new ChangedQualificationsViewModel
        {
            Filter = new NewQualificationFilterViewModel(),
            PaginationViewModel = new PaginationViewModel { RecordsPerPage = 20 }
        };

        await controller.Search(vm);

        var session = controller.HttpContext.Session
            .GetObject<QualificationFilterSessionModel>("ChangedQualificationFilters");

        Assert.Equal(1, session.PageNumber);
    }

    [Fact]
    public async Task Search_MapsNullFieldsCorrectly()
    {
        var controller = CreateController();

        var vm = new ChangedQualificationsViewModel
        {
            Filter = new NewQualificationFilterViewModel(),
            PaginationViewModel = new PaginationViewModel { RecordsPerPage = 10 }
        };

        await controller.Search(vm);

        var session = controller.HttpContext.Session
            .GetObject<QualificationFilterSessionModel>("ChangedQualificationFilters");

        Assert.Equal("", session.QualificationName);
        Assert.Equal("", session.Organisation);
        Assert.Equal("", session.QAN);
    }

    [Fact]
    public async Task Search_Exception_ReturnsIndexView()
    {
        var controller = CreateController();

        var vm = new ChangedQualificationsViewModel
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
}