using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Application.Commands.Application.Review;
using SFA.DAS.AODP.Application.Commands.Review;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Models.Applications;
using SFA.DAS.AODP.Web.Models.BulkActions;
using SFA.DAS.AODP.Web.Models.BulkActions.Options;
using SFA.DAS.AODP.Web.Validators.Messages;
using System.Text.Json;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class ApplicationsReviewControllerBulkActionTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<ApplicationsReviewController>> _loggerMock;
    private readonly Mock<IUserHelperService> _userHelperMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly IOptions<AodpConfiguration> _aodpOptions;
    private readonly ApplicationsReviewController _controller;
    private readonly Mock<IUrlHelper> _urlHelperMock;
    private readonly Mock<IFileService> _fileServiceMock = new();

    public ApplicationsReviewControllerBulkActionTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _loggerMock = _fixture.Freeze<Mock<ILogger<ApplicationsReviewController>>>();
        _userHelperMock = _fixture.Freeze<Mock<IUserHelperService>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();

        _aodpOptions = Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = "https://find-a-qualification.services.ofqual.gov.uk/qualifications/"
        });

        _controller = new ApplicationsReviewController(
            _loggerMock.Object,
            _mediatorMock.Object,
            _userHelperMock.Object,
            _fileServiceMock.Object,
            _aodpOptions);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _controller.TempData = new TempDataDictionary(
            _controller.HttpContext,
            Mock.Of<ITempDataProvider>());

        _userHelperMock
            .Setup(x => x.GetUserType())
            .Returns(UserType.Qfau);

        _userHelperMock
            .Setup(x => x.GetUserEmail())
            .Returns("user@test.com");

        _userHelperMock
            .Setup(x => x.GetUserDisplayName())
            .Returns("Test User");

        _urlHelperMock = new Mock<IUrlHelper>();

        _urlHelperMock
            .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("/review/application-reviews?pageNumber=1&recordsPerPage=10");

        _controller.Url = _urlHelperMock.Object;
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToIndex_AndSetsSuccessFlag_WhenAssignHasNoErrors()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = ids,
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = SubmitAction.Assign,
                Reviewer1 = "reviewer1@test.com",
                Reviewer2 = "reviewer2@test.com"
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 2,
            RecordsPerPage = 20,
            ApplicationSearch = "app",
            AwardingOrganisationSearch = "org"
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<BulkSaveReviewerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkSaveReviewerCommandResponse>
            {
                Success = true,
                Value = new BulkSaveReviewerCommandResponse
                {
                    ErrorCount = 0
                }
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ApplicationsReviewController.Index), redirect.ActionName);
        Assert.True((bool)_controller.TempData[BulkActionApplications.SuccessKey]!);
    }

    [Fact]
    public async Task ApplyBulkAction_SendsBulkSaveReviewerCommand_WithExpectedValues_WhenAssign()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = ids,
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = SubmitAction.Assign,
                Reviewer1 = "reviewer1@test.com",
                Reviewer2 = "reviewer2@test.com"
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<BulkSaveReviewerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkSaveReviewerCommandResponse>
            {
                Success = true,
                Value = new BulkSaveReviewerCommandResponse
                {
                    ErrorCount = 0
                }
            });

        await _controller.ApplyBulkAction(model, query);

        _mediatorMock.Verify(x => x.Send(
            It.Is<BulkSaveReviewerCommand>(cmd =>
                cmd.ApplicationReviewIds.SequenceEqual(ids) &&
                cmd.Reviewer1Set &&
                cmd.Reviewer1 == "reviewer1@test.com" &&
                cmd.Reviewer2Set &&
                cmd.Reviewer2 == "reviewer2@test.com" &&
                cmd.UserType == UserType.Qfau.ToString() &&
                cmd.SentByEmail == "user@test.com" &&
                cmd.SentByName == "Test User"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApplyBulkAction_SendsBulkSaveReviewerCommand_WithNullReviewer_WhenUnassignedSelected()
    {
        var id = Guid.NewGuid();

        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = new List<Guid> { id },
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = SubmitAction.Assign,
                Reviewer1 = ReviewerDropdown.UnassignedValue,
                Reviewer2 = string.Empty
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<BulkSaveReviewerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkSaveReviewerCommandResponse>
            {
                Success = true,
                Value = new BulkSaveReviewerCommandResponse
                {
                    ErrorCount = 0
                }
            });

        await _controller.ApplyBulkAction(model, query);

        _mediatorMock.Verify(x => x.Send(
            It.Is<BulkSaveReviewerCommand>(cmd =>
                cmd.ApplicationReviewIds.SequenceEqual(new[] { id }) &&
                cmd.Reviewer1Set &&
                cmd.Reviewer1 == null &&
                !cmd.Reviewer2Set &&
                cmd.Reviewer2 == null),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToBulkApplicationError_WhenAssignHasErrors()
    {
        var id = Guid.NewGuid();

        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = new List<Guid> { id },
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = SubmitAction.Assign,
                Reviewer1 = "reviewer1@test.com"
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<BulkSaveReviewerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkSaveReviewerCommandResponse>
            {
                Success = true,
                Value = new BulkSaveReviewerCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkReviewerErrorDto>
                    {
                        new()
                        {
                            ReferenceNumber = 1,
                            Qan = "12345678",
                            Title = "Test application",
                            AwardingOrganisation = "AO",
                            ErrorType = BulkReviewerErrorType.Missing
                        }
                    }
                }
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ApplicationsReviewController.BulkApplicationError), redirect.ActionName);
    }

    [Fact]
    public async Task ApplyBulkAction_StoresSerializedErrorModelInTempData_WhenAssignHasErrors()
    {
        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = new List<Guid> { Guid.NewGuid() },
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = SubmitAction.Assign,
                Reviewer1 = "reviewer1@test.com"
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<BulkSaveReviewerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkSaveReviewerCommandResponse>
            {
                Success = true,
                Value = new BulkSaveReviewerCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkReviewerErrorDto>
                    {
                        new()
                        {
                            ReferenceNumber = 1,
                            Qan = "12345678",
                            Title = "Test application",
                            AwardingOrganisation = "AO",
                            ErrorType = BulkReviewerErrorType.Conflict
                        }
                    }
                }
            });

        await _controller.ApplyBulkAction(model, query);

        var json = Assert.IsType<string>(_controller.TempData[BulkActionApplications.Errors]);
        var errorModel = JsonSerializer.Deserialize<ApplicationBulkActionErrorModel>(json);

        Assert.NotNull(errorModel);
        Assert.Single(errorModel.Failed);

        var failed = errorModel.Failed.Single();
        Assert.Equal("000001", failed.ApplicationReference);
        Assert.Equal("12345678", failed.Qan);
        Assert.Equal("Test application", failed.Title);
        Assert.Equal("AO", failed.AwardingOrganisation);
        Assert.Equal("Reviewer 1 and 2 must be different users.", failed.FailureReason);

        Assert.Equal("Go back to Applications", errorModel.BackLinkText);
        Assert.Equal("/review/application-reviews?pageNumber=1&recordsPerPage=10", errorModel.BackLinkUrl);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToIndex_AndSetsSuccessFlag_WhenMessageHasNoErrors()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = ids,
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = SubmitAction.Message,
                BulkActionType = BulkApplicationActionType.ShareWithOfqual
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 2,
            RecordsPerPage = 20
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<BulkApplicationActionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkApplicationActionCommandResponse>
            {
                Success = true,
                Value = new BulkApplicationActionCommandResponse
                {
                    ErrorCount = 0
                }
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ApplicationsReviewController.Index), redirect.ActionName);
        Assert.True((bool)_controller.TempData[BulkActionApplications.SuccessKey]!);
    }

    [Fact]
    public async Task ApplyBulkAction_SendsBulkApplicationActionCommand_WithExpectedValues_WhenMessage()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = ids,
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = SubmitAction.Message,
                BulkActionType = BulkApplicationActionType.ShareWithSkillsEngland
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<BulkApplicationActionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkApplicationActionCommandResponse>
            {
                Success = true,
                Value = new BulkApplicationActionCommandResponse
                {
                    ErrorCount = 0
                }
            });

        await _controller.ApplyBulkAction(model, query);

        _mediatorMock.Verify(x => x.Send(
            It.Is<BulkApplicationActionCommand>(cmd =>
                cmd.ApplicationReviewIds.SequenceEqual(ids) &&
                cmd.ActionType == BulkApplicationActionType.ShareWithSkillsEngland &&
                cmd.UserType == UserType.Qfau.ToString() &&
                cmd.SentByEmail == "user@test.com" &&
                cmd.SentByName == "Test User"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToBulkApplicationError_WhenMessageHasErrors()
    {
        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = new List<Guid> { Guid.NewGuid() },
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = SubmitAction.Message,
                BulkActionType = BulkApplicationActionType.Unlock
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<BulkApplicationActionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkApplicationActionCommandResponse>
            {
                Success = true,
                Value = new BulkApplicationActionCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkApplicationActionErrorDto>
                    {
                        new()
                        {
                            ReferenceNumber = 2,
                            Qan = "87654321",
                            Title = "Message application",
                            AwardingOrganisation = "AO2",
                            ErrorType = BulkApplicationActionErrorType.UpdateFailed
                        }
                    }
                }
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ApplicationsReviewController.BulkApplicationError), redirect.ActionName);
    }

    [Fact]
    public async Task ApplyBulkAction_MapsInvalidActionReason_WhenMessageHasErrors()
    {
        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = new List<Guid> { Guid.NewGuid() },
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = SubmitAction.Message,
                BulkActionType = BulkApplicationActionType.Unlock
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<BulkApplicationActionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkApplicationActionCommandResponse>
            {
                Success = true,
                Value = new BulkApplicationActionCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkApplicationActionErrorDto>
                    {
                        new()
                        {
                            ReferenceNumber = 3,
                            Qan = "11111111",
                            Title = "Invalid action application",
                            AwardingOrganisation = "AO3",
                            ErrorType = BulkApplicationActionErrorType.InvalidAction
                        }
                    }
                }
            });

        await _controller.ApplyBulkAction(model, query);

        var json = Assert.IsType<string>(_controller.TempData[BulkActionApplications.Errors]);
        var errorModel = JsonSerializer.Deserialize<ApplicationBulkActionErrorModel>(json);

        Assert.NotNull(errorModel);
        Assert.Single(errorModel.Failed);
        Assert.Equal("Invalid action.", errorModel.Failed.Single().FailureReason);
    }

    [Fact]
    public async Task ApplyBulkAction_MapsUpdateFailedReason_WhenMessageHasErrors()
    {
        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = new List<Guid> { Guid.NewGuid() },
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = SubmitAction.Message,
                BulkActionType = BulkApplicationActionType.Unlock
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<BulkApplicationActionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkApplicationActionCommandResponse>
            {
                Success = true,
                Value = new BulkApplicationActionCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkApplicationActionErrorDto>
                    {
                        new()
                        {
                            ReferenceNumber = 4,
                            Qan = "22222222",
                            Title = "Update failed application",
                            AwardingOrganisation = "AO4",
                            ErrorType = BulkApplicationActionErrorType.UpdateFailed
                        }
                    }
                }
            });

        await _controller.ApplyBulkAction(model, query);

        var json = Assert.IsType<string>(_controller.TempData[BulkActionApplications.Errors]);
        var errorModel = JsonSerializer.Deserialize<ApplicationBulkActionErrorModel>(json);

        Assert.NotNull(errorModel);
        Assert.Single(errorModel.Failed);
        Assert.Equal("Update failed.", errorModel.Failed.Single().FailureReason);
    }

    [Fact]
    public async Task ApplyBulkAction_MapsUnknownReason_WhenMessageHasErrors()
    {
        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = new List<Guid> { Guid.NewGuid() },
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = SubmitAction.Message,
                BulkActionType = BulkApplicationActionType.Unlock
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<BulkApplicationActionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkApplicationActionCommandResponse>
            {
                Success = true,
                Value = new BulkApplicationActionCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkApplicationActionErrorDto>
                    {
                        new()
                        {
                            ReferenceNumber = 5,
                            Qan = "33333333",
                            Title = "Unknown application",
                            AwardingOrganisation = "AO5",
                            ErrorType = (BulkApplicationActionErrorType)999
                        }
                    }
                }
            });

        await _controller.ApplyBulkAction(model, query);

        var json = Assert.IsType<string>(_controller.TempData[BulkActionApplications.Errors]);
        var errorModel = JsonSerializer.Deserialize<ApplicationBulkActionErrorModel>(json);

        Assert.NotNull(errorModel);
        Assert.Single(errorModel.Failed);
        Assert.Equal("Unknown error.", errorModel.Failed.Single().FailureReason);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToHomeError_WhenSubmitActionIsUnknown()
    {
        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = new List<Guid> { Guid.NewGuid() },
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = (SubmitAction)999
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        var result = await _controller.ApplyBulkAction(model, query);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToHomeError_WhenExceptionThrown()
    {
        var model = new ApplicationsBulkActionPostModel
        {
            SelectedApplicationReviewIds = new List<Guid> { Guid.NewGuid() },
            BulkActionInputViewModel = new ApplicationsBulkActionInputViewModel
            {
                SubmitAction = SubmitAction.Assign,
                Reviewer1 = "reviewer1@test.com"
            }
        };

        var query = new ApplicationsReviewQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<BulkSaveReviewerCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await _controller.ApplyBulkAction(model, query);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public void BulkApplicationError_ReturnsBulkApplicationErrorView_WhenTempDataExists()
    {
        var errorModel = new ApplicationBulkActionErrorModel
        {
            Failed = new List<ApplicationBulkActionErrorItemViewModel>
            {
                new()
                {
                    ReferenceNumber = 1,
                    Qan = "12345678",
                    Title = "Test application",
                    AwardingOrganisation = "AO",
                    FailureReason = "Application not found."
                }
            },
            BackLinkText = "Go back to Applications",
            BackLinkUrl = "/review/application-reviews?pageNumber=1&recordsPerPage=10"
        };

        _controller.TempData[BulkActionApplications.Errors] = JsonSerializer.Serialize(errorModel);

        var result = _controller.BulkApplicationError();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("BulkApplicationError", viewResult.ViewName);

        var model = Assert.IsType<ApplicationBulkActionErrorModel>(viewResult.Model);
        Assert.Single(model.Failed);
        Assert.Equal("000001", model.Failed.Single().ApplicationReference);
        Assert.Equal("12345678", model.Failed.Single().Qan);
        Assert.Equal("Test application", model.Failed.Single().Title);
        Assert.Equal("AO", model.Failed.Single().AwardingOrganisation);
        Assert.Equal("Application not found.", model.Failed.Single().FailureReason);
        Assert.Equal("Go back to Applications", model.BackLinkText);
        Assert.Equal("/review/application-reviews?pageNumber=1&recordsPerPage=10", model.BackLinkUrl);
    }

    [Fact]
    public void BulkApplicationError_ReturnsErrorView_WhenTempDataMissing()
    {
        var result = _controller.BulkApplicationError();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
    }

    [Fact]
    public void BulkApplicationError_ReturnsErrorView_WhenTempDataEmpty()
    {
        _controller.TempData[BulkActionApplications.Errors] = string.Empty;

        var result = _controller.BulkApplicationError();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
    }
}