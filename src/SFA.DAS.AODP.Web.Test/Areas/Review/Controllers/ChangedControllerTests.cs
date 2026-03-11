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
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;
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

    private readonly IOptions<AodpConfiguration> _options =
        Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = DefaultFindQualificationUrl
        });

    private ChangedController CreateController()
    {
        var controller = new ChangedController(_logger.Object, _options, _mediator.Object, _userHelper.Object);

        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.Name, DefaultUserName) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

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
            ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus
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
            response.ProcessStatuses.Add(new GetProcessStatusesQueryResponse.ProcessStatus
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

    [Fact]
    public async Task QualificationDetails_Get_ReturnsRedirect_WhenQualificationReferenceNull()
    {
        var controller = CreateController();

        var result = await controller.QualificationDetails(qualificationReference: (string?)null);

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
    public async Task QualificationDetails_Get_WhenVersionGreaterThanOne_LoadsPreviousVersion_AndPopulatesKeyFieldChanges()
    {
        var controller = CreateController();

        var latestVersion = CreateQualificationDetailsResponse(
            DefaultQan,
            version: VersionTwo,
            qualificationName: "New Qualification Name",
            organisationName: "New Organisation",
            changedFieldNames: "Title,OrganisationName");

        var previousVersion = CreateQualificationDetailsResponse(
            DefaultQan,
            version: VersionOne,
            qualificationName: "Old Qualification Name",
            organisationName: "Old Organisation");

        var processStatusesResponse = CreateProcessStatusesResponse((Guid.NewGuid(), DecisionRequiredStatus));

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string>());
        SetupQualificationDetailsDependencies(latestVersion, processStatusesResponse);

        _mediator.Setup(m => m.Send(It.IsAny<GetQualificationVersionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationDetailsQueryResponse>
            {
                Success = true,
                Value = previousVersion
            });

        var result = await controller.QualificationDetails(DefaultQan);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ChangedQualificationDetailsViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.Equal(VersionTwo, model.Version);
            Assert.NotNull(model.KeyFieldChanges);
            Assert.NotEmpty(model.KeyFieldChanges);
            Assert.Contains(model.KeyFieldChanges, k => k.Name == "Title");
            Assert.Contains(model.KeyFieldChanges, k => k.Name == "Organisation Name");
        });
    }

    [Fact]
    public async Task QualificationDetails_Post_AddsComment_WhenNoProcStatusAndNote()
    {
        var controller = CreateController();

        var model = new ChangedQualificationDetailsViewModel
        {
            Qual = new ChangedQualificationDetailsViewModel.Qualification { Qan = DefaultQan },
            AdditionalActions = new ChangedQualificationDetailsViewModel.AdditionalFormActions
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
            Qual = new ChangedQualificationDetailsViewModel.Qualification { Qan = DefaultQan },
            AdditionalActions = new ChangedQualificationDetailsViewModel.AdditionalFormActions
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
            Qual = new ChangedQualificationDetailsViewModel.Qualification { Qan = DefaultQan },
            AdditionalActions = new ChangedQualificationDetailsViewModel.AdditionalFormActions
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
            Qual = new ChangedQualificationDetailsViewModel.Qualification { Qan = DefaultQan },
            Version = VersionOne,
            AdditionalActions = new ChangedQualificationDetailsViewModel.AdditionalFormActions
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
            Qual = new ChangedQualificationDetailsViewModel.Qualification { Qan = DefaultQan },
            Version = VersionOne,
            AdditionalActions = new ChangedQualificationDetailsViewModel.AdditionalFormActions
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
    public async Task Search_RedirectsToIndex_WithMappedRouteValues()
    {
        var controller = CreateController();

        var viewModel = new ChangedQualificationsViewModel
        {
            PaginationViewModel = new PaginationViewModel
            {
                RecordsPerPage = DefaultRecordsPerPage
            },
            Filter = new NewQualificationFilterViewModel
            {
                QualificationName = SearchName,
                Organisation = SearchOrganisation,
                QAN = SearchQan,
                ProcessStatusIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            }
        };

        var result = await controller.Search(viewModel);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(ChangedController.Index), redirect.ActionName);
            Assert.Equal(ResetPageNumber, redirect.RouteValues!["pageNumber"]);
            Assert.Equal(DefaultRecordsPerPage, redirect.RouteValues["recordsPerPage"]);
            Assert.Equal(SearchName, redirect.RouteValues["name"]);
            Assert.Equal(SearchOrganisation, redirect.RouteValues["organisation"]);
            Assert.Equal(SearchQan, redirect.RouteValues["qan"]);
            Assert.Equal(viewModel.Filter.ProcessStatusIds, redirect.RouteValues["processStatusIds"]);
        });
    }

    [Fact]
    public async Task Clear_RedirectsToIndex_WhenModelStateValid()
    {
        var controller = CreateController();

        var result = await controller.Clear(DefaultRecordsPerPage);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(ChangedController.Index), redirect.ActionName);
            Assert.Equal(ClearedPageNumber, redirect.RouteValues!["pageNumber"]);
            Assert.Equal(DefaultRecordsPerPage, redirect.RouteValues["recordsPerPage"]);
        });
    }

    [Fact]
    public async Task Clear_ReturnsIndexView_WhenModelStateInvalid()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("recordsPerPage", "invalid");

        var result = await controller.Clear(DefaultRecordsPerPage);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal("Index", view.ViewName);
        });
    }

    [Fact]
    public async Task ChangePage_RedirectsToIndex_WithOverriddenPageNumber()
    {
        var controller = CreateController();

        var qualificationQuery = new QualificationQuery
        {
            PageNumber = DefaultPageNumber,
            RecordsPerPage = DefaultRecordsPerPage,
            Name = SearchName,
            Organisation = SearchOrganisation,
            Qan = SearchQan
        };

        var result = await controller.ChangePage(qualificationQuery, ChangedPageNumber);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(ChangedController.Index), redirect.ActionName);
            Assert.Equal(ChangedPageNumber, redirect.RouteValues!["pageNumber"]);
            Assert.Equal(DefaultRecordsPerPage, redirect.RouteValues["recordsPerPage"]);
            Assert.Equal(SearchName, redirect.RouteValues["name"]);
            Assert.Equal(SearchOrganisation, redirect.RouteValues["organisation"]);
            Assert.Equal(SearchQan, redirect.RouteValues["qan"]);
        });
    }

    [Fact]
    public async Task ChangePage_ReturnsIndexView_WhenModelStateInvalid()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("pageNumber", "invalid");

        var qualificationQuery = new QualificationQuery
        {
            PageNumber = DefaultPageNumber,
            RecordsPerPage = DefaultRecordsPerPage
        };

        var result = await controller.ChangePage(qualificationQuery, ChangedPageNumber);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal("Index", view.ViewName);
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
    public async Task Index_ReturnsView_WithModel()
    {
        var controller = CreateController();

        var qualificationQuery = new QualificationQuery
        {
            PageNumber = DefaultPageNumber,
            RecordsPerPage = DefaultRecordsPerPage,
            Name = SearchName,
            Organisation = SearchOrganisation,
            Qan = SearchQan
        };

        var processStatusesResponse = CreateProcessStatusesResponse((Guid.NewGuid(), DecisionRequiredStatus));

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = processStatusesResponse
            });

        _mediator.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetChangedQualificationsQueryResponse>
            {
                Success = true,
                Value = new GetChangedQualificationsQueryResponse
                {
                    Data = new List<ChangedQualification>(),
                    TotalRecords = 0,
                    Skip = DefaultRecordsPerPage,
                    Take = DefaultRecordsPerPage
                }
            });

        var result = await controller.Index(qualificationQuery);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ChangedQualificationsViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.NotNull(model);
            Assert.NotNull(model.Filter);
            Assert.Equal(SearchName, model.Filter.QualificationName);
            Assert.Equal(SearchOrganisation, model.Filter.Organisation);
            Assert.Equal(SearchQan, model.Filter.QAN);
            Assert.Equal(DefaultFindQualificationUrl, model.FindRegulatedQualificationUrl);
        });
    }

    [Fact]
    public async Task Index_RedirectsToHomeError_WhenExceptionThrown()
    {
        var controller = CreateController();

        var qualificationQuery = new QualificationQuery
        {
            PageNumber = DefaultPageNumber,
            RecordsPerPage = DefaultRecordsPerPage
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await controller.Index(qualificationQuery);

        var redirect = Assert.IsType<RedirectResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal("/Home/Error", redirect.Url);
        });
    }
}