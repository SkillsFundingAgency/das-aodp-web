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
using Xunit;
using static SFA.DAS.AODP.Application.Queries.Qualifications.GetDiscussionHistoriesForQualificationQueryResponse;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class NewControllerTests
{
    private const string FindRegulatedQualificationUrl = "http://find";

    private const string QualificationReference = "61054902";
    private const string EmptyQualificationReference = "";
    private const string ShortQualificationReference = "Q";

    private const string QualificationName = "Test Qualification";
    private const string AwardingOrganisation = "AO";
    private const string QualificationTitle = "T";
    private const string QualificationStatus = "S";
    private const string QualificationAgeGroup = "AG";
    private const string QualificationReferenceCode = "R";

    private const string SearchName = "name";
    private const string SearchOrganisation = "org";
    private const string SearchQan = "qan";

    private const string ChangedPageName = "n";
    private const string ChangedPageOrganisation = "o";
    private const string ChangedPageQan = "q";

    private const string CommentNote = "note";
    private const string DraftStage = "Draft";
    private const string DecisionRequiredStatus = "Decision Required";
    private const string NoActionRequiredStatus = "No Action Required";
    private const string NotAllowedStatus = "Not Allowed";
    private const string QualificationSearchReturnTo = "QualificationSearch";

    private const string ReviewArea = "Review";
    private const string NewControllerName = "New";
    private const string QualificationSearchController = "QualificationSearch";
    private const string IndexAction = "Index";
    private const string IndexViewName = "Index";
    private const string CsvContentType = "text/csv";
    private const string ErrorUrl = "/Home/Error";

    private const int DefaultRecordsPerPage = 10;
    private const int PageNumberZero = 0;
    private const int PageNumberOne = 1;
    private const int PageNumberTwo = 2;

    private readonly Mock<ILogger<NewController>> _logger = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly Mock<IUserHelperService> _userHelper = new();
    private readonly IOptions<AodpConfiguration> _options =
        Options.Create(new AodpConfiguration { FindRegulatedQualificationUrl = FindRegulatedQualificationUrl });

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
        return new BaseMediatrResponse<T>
        {
            Success = true,
            Value = value
        };
    }

    [Fact]
    public async Task Index_PageNumberZero_ReturnsViewWithDefaultModel()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new GetProcessStatusesQueryResponse
            {
                ProcessStatuses = new List<GetProcessStatusesQueryResponse.ProcessStatus>()
            }));

        var qualificationQuery = new QualificationQuery
        {
            PageNumber = PageNumberZero
        };

        var result = await controller.Index(qualificationQuery);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NewQualificationsViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.Empty(model.NewQualifications);
            Assert.Equal(FindRegulatedQualificationUrl, model.FindRegulatedQualificationUrl);
            Assert.NotNull(model.Filter);
            Assert.NotNull(model.ProcessStatuses);
        });
    }

    [Fact]
    public async Task Index_PageNumberGreaterThanZero_FillsModelFromMediator()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new GetProcessStatusesQueryResponse
            {
                ProcessStatuses = new List<GetProcessStatusesQueryResponse.ProcessStatus>()
            }));

        var newQualsResponse = new GetNewQualificationsQueryResponse
        {
            TotalRecords = 1,
            Skip = 0,
            Take = DefaultRecordsPerPage,
            Data = new List<NewQualification>
            {
                new()
                {
                    Title = QualificationTitle,
                    Reference = QualificationReferenceCode,
                    AwardingOrganisation = AwardingOrganisation,
                    Status = QualificationStatus,
                    AgeGroup = QualificationAgeGroup
                }
            },
            Job = new Job
            {
                Name = "job",
                Status = "OK"
            }
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(newQualsResponse));

        var qualificationQuery = new QualificationQuery
        {
            PageNumber = PageNumberOne
        };

        var result = await controller.Index(qualificationQuery);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NewQualificationsViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.Single(model.NewQualifications);
            Assert.Equal(QualificationTitle, model.NewQualifications.First().Title);
            Assert.Equal(FindRegulatedQualificationUrl, model.FindRegulatedQualificationUrl);
            Assert.NotNull(model.Filter);
            Assert.NotNull(model.ProcessStatuses);
        });
    }

    [Fact]
    public async Task Search_Post_RedirectsToIndexWithQuery()
    {
        var controller = CreateController();

        var processStatusIds = new List<Guid> { Guid.NewGuid() };

        var viewModel = new NewQualificationsViewModel();
        viewModel.PaginationViewModel.RecordsPerPage = DefaultRecordsPerPage;
        viewModel.Filter.QualificationName = SearchName;
        viewModel.Filter.Organisation = SearchOrganisation;
        viewModel.Filter.QAN = SearchQan;
        viewModel.Filter.ProcessStatusIds = processStatusIds;

        var result = await controller.Search(viewModel);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(NewController.Index), redirect.ActionName);
            Assert.Equal(PageNumberOne, redirect.RouteValues!["pageNumber"]);
            Assert.Equal(DefaultRecordsPerPage, redirect.RouteValues["recordsPerPage"]);
            Assert.Equal(SearchName, redirect.RouteValues["name"]);
            Assert.Equal(SearchOrganisation, redirect.RouteValues["organisation"]);
            Assert.Equal(SearchQan, redirect.RouteValues["qan"]);
            Assert.Equal(processStatusIds, redirect.RouteValues["processStatusIds"]);
        });
    }

    [Fact]
    public async Task Clear_ValidModelState_RedirectsToIndex()
    {
        var controller = CreateController();

        var result = await controller.Clear(DefaultRecordsPerPage);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(NewController.Index), redirect.ActionName);
            Assert.Equal(PageNumberZero, redirect.RouteValues!["pageNumber"]);
            Assert.Equal(DefaultRecordsPerPage, redirect.RouteValues["recordsPerPage"]);
        });
    }

    [Fact]
    public async Task Clear_InvalidModelState_ReturnsIndexView()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("k", "e");

        var result = await controller.Clear(DefaultRecordsPerPage);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(IndexViewName, view.ViewName);
        });
    }

    [Fact]
    public async Task ChangePage_ValidModelState_RedirectsToIndex()
    {
        var controller = CreateController();

        var qualificationQuery = new QualificationQuery
        {
            PageNumber = PageNumberTwo,
            RecordsPerPage = DefaultRecordsPerPage,
            Name = ChangedPageName,
            Organisation = ChangedPageOrganisation,
            Qan = ChangedPageQan
        };

        var result = await controller.ChangePage(qualificationQuery);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(NewController.Index), redirect.ActionName);
            Assert.Equal(PageNumberOne, redirect.RouteValues!["pageNumber"]);
            Assert.Equal(DefaultRecordsPerPage, redirect.RouteValues["recordsPerPage"]);
            Assert.Equal(ChangedPageName, redirect.RouteValues["name"]);
            Assert.Equal(ChangedPageOrganisation, redirect.RouteValues["organisation"]);
            Assert.Equal(ChangedPageQan, redirect.RouteValues["qan"]);
        });
    }

    [Fact]
    public async Task ChangePage_InvalidModelState_ReturnsIndexView()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("k", "e");

        var qualificationQuery = new QualificationQuery
        {
            PageNumber = PageNumberTwo,
            RecordsPerPage = DefaultRecordsPerPage,
            Name = ChangedPageName,
            Organisation = ChangedPageOrganisation,
            Qan = ChangedPageQan
        };

        var result = await controller.ChangePage(qualificationQuery);

        var view = Assert.IsType<ViewResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(IndexViewName, view.ViewName);
        });
    }

    [Fact]
    public async Task QualificationDetails_Get_WithEmptyRef_RedirectsToError()
    {
        var controller = CreateController();

        var result = await controller.QualificationDetails(EmptyQualificationReference);

        var redirect = Assert.IsType<RedirectResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(ErrorUrl, redirect.Url);
        });
    }

    [Fact]
    public async Task QualificationDetails_Post_WithNote_SendsAddDiscussionCommand_AndRedirects()
    {
        var controller = CreateController();

        var posted = new NewQualificationDetailsViewModel
        {
            Qual = new NewQualificationDetailsViewModel.Qualification { Qan = ShortQualificationReference },
            AdditionalActions = new NewQualificationDetailsViewModel.AdditionalFormActions
            {
                Note = CommentNote
            }
        };

        _mediator.Setup(m => m.Send(It.IsAny<AddQualificationDiscussionHistoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new EmptyResponse()));

        var result = await controller.QualificationDetails(posted);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(NewController.QualificationDetails), redirect.ActionName);
            Assert.Equal(ShortQualificationReference, redirect.RouteValues!["qualificationReference"]);
            Assert.True(controller.TempData.ContainsKey(NewController.NewQualDataKeys.CommentSaved.ToString()));
        });
    }

    [Fact]
    public async Task QualificationDetails_Post_WithNoNoteOrStatus_RedirectsToGet()
    {
        var controller = CreateController();

        var posted = new NewQualificationDetailsViewModel
        {
            Qual = new NewQualificationDetailsViewModel.Qualification { Qan = ShortQualificationReference },
            AdditionalActions = new NewQualificationDetailsViewModel.AdditionalFormActions
            {
                Note = string.Empty,
                ProcessStatusId = null
            }
        };

        var result = await controller.QualificationDetails(posted);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(NewController.QualificationDetails), redirect.ActionName);
            Assert.Equal(ShortQualificationReference, redirect.RouteValues!["qualificationReference"]);
        });
    }

    [Fact]
    public async Task QualificationDetails_Post_WithStatus_UserNotAllowed_RedirectsToGet()
    {
        var controller = CreateController();

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string>());

        var processStatusId = Guid.NewGuid();

        var posted = new NewQualificationDetailsViewModel
        {
            Qual = new NewQualificationDetailsViewModel.Qualification { Qan = ShortQualificationReference },
            AdditionalActions = new NewQualificationDetailsViewModel.AdditionalFormActions
            {
                ProcessStatusId = processStatusId
            },
            ProcessStatuses = new List<NewQualificationDetailsViewModel.ProcessStatus>
        {
            new()
            {
                Id = processStatusId,
                Name = NotAllowedStatus
            }
        },
            Stage = new NewQualificationDetailsViewModel.LifecycleStage
            {
                Name = DraftStage
            }
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new GetProcessStatusesQueryResponse
            {
                ProcessStatuses = new List<GetProcessStatusesQueryResponse.ProcessStatus>
                {
                new()
                {
                    Id = processStatusId,
                    Name = NotAllowedStatus
                }
                }
            }));

        var result = await controller.QualificationDetails(posted);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(NewController.QualificationDetails), redirect.ActionName);
            Assert.Equal(ShortQualificationReference, redirect.RouteValues!["qualificationReference"]);
        });

        _mediator.Verify(
            m => m.Send(It.IsAny<UpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task QualificationDetailsTimeline_EmptyRef_RedirectsToError()
    {
        var controller = CreateController();

        var result = await controller.QualificationDetailsTimeline(EmptyQualificationReference);

        var redirect = Assert.IsType<RedirectResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(ErrorUrl, redirect.Url);
        });
    }

    [Fact]
    public async Task QualificationDetails_Get_ReturnsView_WithModel_And_DefaultBackData()
    {
        var controller = CreateController();

        var qualificationResponse = new GetQualificationDetailsQueryResponse
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            VersionFieldChangesId = Guid.NewGuid(),
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = QualificationReference,
                QualificationName = QualificationName,
                Versions = new List<GetQualificationDetailsQueryResponse>()
            },
            ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus
            {
                Id = Guid.NewGuid(),
                Name = DecisionRequiredStatus
            },
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage
            {
                Id = Guid.NewGuid(),
                Name = DraftStage
            },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation
            {
                Id = Guid.NewGuid(),
                NameOfqual = AwardingOrganisation
            },
            Version = 1,
            LastUpdatedDate = DateTime.UtcNow,
            UiLastUpdatedDate = DateTime.UtcNow,
            InsertedDate = DateTime.UtcNow
        };

        var processStatusesResponse = new GetProcessStatusesQueryResponse();
        processStatusesResponse.ProcessStatuses.Add(new GetProcessStatusesQueryResponse.ProcessStatus
        {
            Id = Guid.NewGuid(),
            Name = DecisionRequiredStatus
        });

        var feedbackResponse = new GetFeedbackForQualificationFundingByIdQueryResponse
        {
            QualificationFundedOffers = new List<GetFeedbackForQualificationFundingByIdQueryResponse.QualificationFunding>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    FundedOfferName = "Offer 1",
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    EndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(10))
                }
            }
        };

        var applicationsResponse = new GetApplicationsByQanQueryResponse
        {
            Applications = new List<GetApplicationsByQanQueryResponse.Application>
            {
                new()
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

        var result = await controller.QualificationDetails(QualificationReference);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NewQualificationDetailsViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.Equal(QualificationReference, model.Qual.Qan);
            Assert.NotNull(model.ProcessStatuses);
            Assert.NotEmpty(model.ProcessStatuses);
            Assert.NotNull(model.Applications);
            Assert.NotEmpty(model.Applications);
            Assert.NotNull(model.FundingDetails);
            Assert.NotEmpty(model.FundingDetails);
            Assert.Equal(ReviewArea, model.BackArea);
            Assert.Equal(NewControllerName, model.BackController);
            Assert.Equal(IndexAction, model.BackAction);
        });
    }

    [Fact]
    public async Task QualificationDetails_Get_WithReturnToQualificationSearch_SetsBackToQualificationSearch()
    {
        var controller = CreateController();

        var qualificationResponse = new GetQualificationDetailsQueryResponse
        {
            Id = Guid.NewGuid(),
            QualificationId = Guid.NewGuid(),
            VersionFieldChangesId = Guid.NewGuid(),
            Qual = new GetQualificationDetailsQueryResponse.Qualification
            {
                Id = Guid.NewGuid(),
                Qan = QualificationReference,
                QualificationName = QualificationName,
                Versions = new List<GetQualificationDetailsQueryResponse>()
            },
            ProcStatus = new GetQualificationDetailsQueryResponse.ProcessStatus
            {
                Id = Guid.NewGuid(),
                Name = DecisionRequiredStatus
            },
            Stage = new GetQualificationDetailsQueryResponse.LifecycleStage
            {
                Id = Guid.NewGuid(),
                Name = DraftStage
            },
            Organisation = new GetQualificationDetailsQueryResponse.AwardingOrganisation
            {
                Id = Guid.NewGuid(),
                NameOfqual = AwardingOrganisation
            },
            Version = 1,
            LastUpdatedDate = DateTime.UtcNow,
            UiLastUpdatedDate = DateTime.UtcNow,
            InsertedDate = DateTime.UtcNow
        };

        var processStatusesResponse = new GetProcessStatusesQueryResponse();
        processStatusesResponse.ProcessStatuses.Add(new GetProcessStatusesQueryResponse.ProcessStatus
        {
            Id = Guid.NewGuid(),
            Name = DecisionRequiredStatus
        });

        var feedbackResponse = new GetFeedbackForQualificationFundingByIdQueryResponse
        {
            QualificationFundedOffers = new List<GetFeedbackForQualificationFundingByIdQueryResponse.QualificationFunding>()
        };

        var applicationsResponse = new GetApplicationsByQanQueryResponse
        {
            Applications = new List<GetApplicationsByQanQueryResponse.Application>()
        };

        _userHelper.Setup(u => u.GetUserRoles()).Returns(new List<string>());

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

        var result = await controller.QualificationDetails(QualificationReference, QualificationSearchReturnTo);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NewQualificationDetailsViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.Equal(QualificationReference, model.Qual.Qan);
            Assert.Equal(ReviewArea, model.BackArea);
            Assert.Equal(QualificationSearchController, model.BackController);
            Assert.Equal(IndexAction, model.BackAction);
            Assert.Equal(QualificationSearchReturnTo, model.ReturnTo);
        });
    }

    [Fact]
    public async Task ExportData_WhenNoExports_RedirectsToError()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetNewQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetQualificationsExportResponse>
            {
                Success = false,
                ErrorMessage = "no"
            });

        var result = await controller.ExportData();

        var redirect = Assert.IsType<RedirectResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(ErrorUrl, redirect.Url);
        });
    }

    [Fact]
    public async Task ExportData_WhenExports_ReturnsFile()
    {
        var controller = CreateController();

        var exportResponse = new GetQualificationsExportResponse
        {
            QualificationExports = new List<QualificationExport>
            {
                new()
                {
                    QualificationNumber = ShortQualificationReference
                }
            }
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetNewQualificationsCsvExportQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(exportResponse));

        var result = await controller.ExportData();

        var file = Assert.IsType<FileContentResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(CsvContentType, file.ContentType);
            Assert.NotEmpty(file.FileContents);
        });
    }

    [Fact]
    public async Task QualificationDetailsTimeline_WithValidReference_ReturnsViewWithModelAndSetsQan()
    {
        var controller = CreateController();

        var timelineResponse = new GetDiscussionHistoriesForQualificationQueryResponse
        {
            QualificationDiscussionHistories = new List<QualificationDiscussionHistory>()
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetDiscussionHistoriesForQualificationQuery>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new BaseMediatrResponse<GetDiscussionHistoriesForQualificationQueryResponse>
            {
                Success = true,
                Value = timelineResponse
            }));

        var result = await controller.QualificationDetailsTimeline(QualificationReference);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationDetailsTimelineViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.Equal(QualificationReference, model.Qan);
            Assert.NotNull(model.QualificationDiscussionHistories);
            Assert.Empty(model.QualificationDiscussionHistories);
        });
    }

    [Fact]
    public async Task QualificationDetailsTimeline_WithValidReferenceAndHistories_ReturnsViewWithHistories()
    {
        var controller = CreateController();

        var timelineResponse = new GetDiscussionHistoriesForQualificationQueryResponse
        {
            QualificationDiscussionHistories = new List<GetDiscussionHistoriesForQualificationQueryResponse.QualificationDiscussionHistory>
        {
            new()
            {
                Id = Guid.NewGuid(),
                QualificationId = Guid.NewGuid(),
                ActionTypeId = Guid.NewGuid(),
                Title = "Change",
                Notes = "A note",
                UserDisplayName = "Test User",
                Timestamp = DateTime.UtcNow,
                ActionType = new ActionType
                {
                    Id = Guid.NewGuid(),
                    Description = "Changed"
                }
            }
        }
        };

        _mediator.Setup(m => m.Send(It.IsAny<GetDiscussionHistoriesForQualificationQuery>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new BaseMediatrResponse<GetDiscussionHistoriesForQualificationQueryResponse>
            {
                Success = true,
                Value = timelineResponse
            }));

        var result = await controller.QualificationDetailsTimeline(QualificationReference);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationDetailsTimelineViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.Equal(QualificationReference, model.Qan);
            Assert.Single(model.QualificationDiscussionHistories);
            Assert.Equal("Change", model.QualificationDiscussionHistories.First().Title);
            Assert.Equal("A note", model.QualificationDiscussionHistories.First().Notes);
            Assert.Equal("Test User", model.QualificationDiscussionHistories.First().UserDisplayName);
            Assert.NotNull(model.QualificationDiscussionHistories.First().ActionType);
            Assert.Equal("Changed", model.QualificationDiscussionHistories.First().ActionType.Description);
        });
    }

    [Fact]
    public async Task QualificationDetailsTimeline_WhenMediatorThrows_RedirectsToQualificationDetails()
    {
        var controller = CreateController();

        _mediator.Setup(m => m.Send(It.IsAny<GetDiscussionHistoriesForQualificationQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("test"));

        var result = await controller.QualificationDetailsTimeline(QualificationReference);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(NewController.QualificationDetails), redirect.ActionName);
            Assert.Equal(QualificationReference, redirect.RouteValues!["qualificationReference"]);
        });
    }
}