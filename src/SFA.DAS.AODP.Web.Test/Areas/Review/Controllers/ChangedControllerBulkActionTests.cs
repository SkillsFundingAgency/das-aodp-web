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
using SFA.DAS.AODP.Application.Commands.Qualifications;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.BulkActions;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Security.Claims;
using System.Text.Json;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class ChangedControllerBulkActionTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<ChangedController>> _loggerMock;
    private readonly Mock<IUserHelperService> _userHelperMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly IOptions<AodpConfiguration> _aodpOptions;
    private readonly ChangedController _controller;
    private readonly Mock<IUrlHelper> _urlHelperMock;

    public ChangedControllerBulkActionTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _loggerMock = _fixture.Freeze<Mock<ILogger<ChangedController>>>();
        _userHelperMock = _fixture.Freeze<Mock<IUserHelperService>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();

        _aodpOptions = Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = "https://find-a-qualification.services.ofqual.gov.uk/qualifications/"
        });

        _controller = new ChangedController(
            _loggerMock.Object,
            _aodpOptions,
            _mediatorMock.Object,
            _userHelperMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _controller.TempData = new TempDataDictionary(
            _controller.HttpContext,
            Mock.Of<ITempDataProvider>());

        _userHelperMock
            .Setup(u => u.GetUserRoles())
            .Returns(new List<string>());

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetProcessStatusesQueryResponse>
            {
                Success = true,
                Value = new GetProcessStatusesQueryResponse
                {
                    ProcessStatuses = new List<GetProcessStatusesQueryResponse.ProcessStatus>
                    {
                        new() { Id = Guid.NewGuid(), Name = "Decision Required" }
                    }
                }
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetChangedQualificationsQueryResponse>
            {
                Success = true,
                Value = new GetChangedQualificationsQueryResponse
                {
                    Data = new List<ChangedQualification>(),
                    TotalRecords = 0,
                    Skip = 0,
                    Take = 10
                }
            });

        _urlHelperMock = new Mock<IUrlHelper>();

        _urlHelperMock
            .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
            .Returns("/Review/Changed/Index?pageNumber=1&recordsPerPage=10");

        _controller.Url = _urlHelperMock.Object;
    }

    [Fact]
    public async Task ApplyBulkAction_ReturnsIndexView_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("BulkAction", "Invalid");

        var model = new ChangedQualificationsViewModel
        {
            SelectedQualificationIds = new List<Guid> { Guid.NewGuid() },
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = Guid.NewGuid(),
                Comment = "bulk comment"
            }
        };

        var query = new QualificationQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        var result = await _controller.ApplyBulkAction(model, query);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);

        var viewModel = Assert.IsType<ChangedQualificationsViewModel>(viewResult.Model);
        Assert.NotNull(viewModel);
        Assert.Equal(model.SelectedQualificationIds, viewModel.SelectedQualificationIds);
        Assert.Equal(model.BulkAction.ProcessStatusId, viewModel.BulkAction.ProcessStatusId);
        Assert.Equal(model.BulkAction.Comment, viewModel.BulkAction.Comment);

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToIndex_AndSetsSuccessFlag_WhenNoErrors()
    {
        var selectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var processStatusId = Guid.NewGuid();

        var model = new ChangedQualificationsViewModel
        {
            SelectedQualificationIds = selectedIds,
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = processStatusId,
                Comment = "bulk comment"
            }
        };

        var query = new QualificationQuery
        {
            PageNumber = 2,
            RecordsPerPage = 20,
            Organisation = "Org",
            Name = "Qual",
            Qan = "123"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse
                {
                    ErrorCount = 0
                }
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ChangedController.Index), redirectResult.ActionName);

        Assert.True((bool)_controller.TempData[BulkActionQualifications.SuccessKey]!);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToIndex_WithRouteValues_WhenNoErrors()
    {
        var model = new ChangedQualificationsViewModel
        {
            SelectedQualificationIds = new List<Guid> { Guid.NewGuid() },
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = Guid.NewGuid(),
                Comment = "bulk comment"
            }
        };

        var query = new QualificationQuery
        {
            PageNumber = 2,
            RecordsPerPage = 20,
            Organisation = "Org",
            Name = "Qual",
            Qan = "123"
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse
                {
                    ErrorCount = 0
                }
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(nameof(ChangedController.Index), redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(query.PageNumber, redirectResult.RouteValues["pageNumber"]);
        Assert.Equal(query.RecordsPerPage, redirectResult.RouteValues["recordsPerPage"]);
        Assert.Equal(query.Organisation, redirectResult.RouteValues["organisation"]);
        Assert.Equal(query.Name, redirectResult.RouteValues["name"]);
        Assert.Equal(query.Qan, redirectResult.RouteValues["qan"]);
    }

    [Fact]
    public async Task ApplyBulkAction_SendsBulkUpdateCommand_WithExpectedValues()
    {
        var selectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var processStatusId = Guid.NewGuid();

        var model = new ChangedQualificationsViewModel
        {
            SelectedQualificationIds = selectedIds,
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = processStatusId,
                Comment = "bulk comment"
            }
        };

        var query = new QualificationQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _controller.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "bob") }));

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse
                {
                    ErrorCount = 0
                }
            });

        await _controller.ApplyBulkAction(model, query);

        _mediatorMock.Verify(m => m.Send(
            It.Is<BulkUpdateQualificationStatusCommand>(cmd =>
                cmd.QualificationIds.SequenceEqual(selectedIds) &&
                cmd.ProcessStatusId == processStatusId &&
                cmd.Comment == "bulk comment" &&
                cmd.UserDisplayName == "bob"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToBulkQualificationError_WhenErrorsExist()
    {
        var selectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var processStatusId = Guid.NewGuid();

        var model = new ChangedQualificationsViewModel
        {
            SelectedQualificationIds = selectedIds,
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = processStatusId,
                Comment = "bulk comment"
            }
        };

        var query = new QualificationQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkUpdateQualificationsErrorDto>
                    {
                    new()
                    {
                        QualificationId = selectedIds[0],
                        Qan = "12345678",
                        Title = "Test qualification",
                        ErrorType = BulkUpdateQualificationsErrorType.Missing
                    }
                    }
                }
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ChangedController.BulkQualificationError), redirectResult.ActionName);
    }

    [Fact]
    public async Task ApplyBulkAction_StoresErrorsInTempData_WhenErrorsExist()
    {
        var selectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var processStatusId = Guid.NewGuid();

        var model = new ChangedQualificationsViewModel
        {
            SelectedQualificationIds = selectedIds,
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = processStatusId,
                Comment = "bulk comment"
            }
        };

        var query = new QualificationQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkUpdateQualificationsErrorDto>
                    {
                    new()
                    {
                        QualificationId = selectedIds[0],
                        Qan = "12345678",
                        Title = "Test qualification",
                        ErrorType = BulkUpdateQualificationsErrorType.Missing
                    }
                    }
                }
            });

        await _controller.ApplyBulkAction(model, query);

        Assert.True(_controller.TempData.ContainsKey(BulkActionQualifications.Errors));
        Assert.NotNull(_controller.TempData[BulkActionQualifications.Errors]);
    }

    [Fact]
    public async Task ApplyBulkAction_StoresSerializedErrorModelInTempData_WhenErrorsExist()
    {
        var selectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var processStatusId = Guid.NewGuid();

        _controller.Url = Mock.Of<IUrlHelper>(u =>
            u.Action(It.IsAny<UrlActionContext>()) == "/Review/Changed/Index?pageNumber=1&recordsPerPage=10");

        var model = new ChangedQualificationsViewModel
        {
            SelectedQualificationIds = selectedIds,
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = processStatusId,
                Comment = "bulk comment"
            }
        };

        var query = new QualificationQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkUpdateQualificationsErrorDto>
                    {
                    new()
                    {
                        QualificationId = selectedIds[0],
                        Qan = "12345678",
                        Title = "Test qualification",
                        ErrorType = BulkUpdateQualificationsErrorType.Missing
                    }
                    }
                }
            });

        await _controller.ApplyBulkAction(model, query);

        var json = Assert.IsType<string>(_controller.TempData[BulkActionQualifications.Errors]);

        var errorModel = JsonSerializer.Deserialize<QualificationBulkActionErrorModel>(json);

        Assert.NotNull(errorModel);
        Assert.Single(errorModel.Failed);

        var failed = errorModel.Failed.Single();
        Assert.Equal(selectedIds[0], failed.QualificationId);
        Assert.Equal("12345678", failed.Qan);
        Assert.Equal("Test qualification", failed.Title);
        Assert.Equal("Qualification not found.", failed.FailureReason);

        Assert.Equal("Go back to Changed qualifications", errorModel.BackLinkText);
        Assert.Equal("/Review/Changed/Index?pageNumber=1&recordsPerPage=10", errorModel.BackLinkUrl);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToHomeError_WhenExceptionThrown()
    {
        var model = new ChangedQualificationsViewModel
        {
            SelectedQualificationIds = new List<Guid> { Guid.NewGuid() },
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = Guid.NewGuid(),
                Comment = "bulk comment"
            }
        };

        var query = new QualificationQuery
        {
            PageNumber = 1,
            RecordsPerPage = 10
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await _controller.ApplyBulkAction(model, query);

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirectResult.Url);
    }

    [Fact]
    public async Task ApplyBulkAction_MapsMissingErrorReason_WhenErrorsExist()
    {
        var qualificationId = Guid.NewGuid();

        _controller.Url = Mock.Of<IUrlHelper>(u =>
            u.Action(It.IsAny<UrlActionContext>()) == "/Review/Changed/Index?pageNumber=1&recordsPerPage=10");

        var model = new ChangedQualificationsViewModel
        {
            SelectedQualificationIds = new List<Guid> { qualificationId },
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = Guid.NewGuid(),
                Comment = "bulk comment"
            }
        };

        var query = new QualificationQuery { PageNumber = 1, RecordsPerPage = 10 };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkUpdateQualificationsErrorDto>
                    {
                    new()
                    {
                        QualificationId = qualificationId,
                        Qan = "12345678",
                        Title = "Missing qualification",
                        ErrorType = BulkUpdateQualificationsErrorType.Missing
                    }
                    }
                }
            });

        await _controller.ApplyBulkAction(model, query);

        var json = Assert.IsType<string>(_controller.TempData[BulkActionQualifications.Errors]);
        var errorModel = JsonSerializer.Deserialize<QualificationBulkActionErrorModel>(json);

        Assert.NotNull(errorModel);
        Assert.Single(errorModel.Failed);
        Assert.Equal("Qualification not found.", errorModel.Failed.Single().FailureReason);
    }

    [Fact]
    public async Task ApplyBulkAction_MapsHistoryFailedErrorReason_WhenErrorsExist()
    {
        var qualificationId = Guid.NewGuid();

        _controller.Url = Mock.Of<IUrlHelper>(u =>
            u.Action(It.IsAny<UrlActionContext>()) == "/Review/Changed/Index?pageNumber=1&recordsPerPage=10");

        var model = new ChangedQualificationsViewModel
        {
            SelectedQualificationIds = new List<Guid> { qualificationId },
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = Guid.NewGuid(),
                Comment = "bulk comment"
            }
        };

        var query = new QualificationQuery { PageNumber = 1, RecordsPerPage = 10 };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkUpdateQualificationsErrorDto>
                    {
                    new()
                    {
                        QualificationId = qualificationId,
                        Qan = "12345678",
                        Title = "History failure qualification",
                        ErrorType = BulkUpdateQualificationsErrorType.HistoryFailed
                    }
                    }
                }
            });

        await _controller.ApplyBulkAction(model, query);

        var json = Assert.IsType<string>(_controller.TempData[BulkActionQualifications.Errors]);
        var errorModel = JsonSerializer.Deserialize<QualificationBulkActionErrorModel>(json);

        Assert.NotNull(errorModel);
        Assert.Single(errorModel.Failed);
        Assert.Equal("Status updated but history was not updated.", errorModel.Failed.Single().FailureReason);
    }

    [Fact]
    public async Task ApplyBulkAction_MapsUnknownErrorReason_WhenErrorsExist()
    {
        var qualificationId = Guid.NewGuid();

        _controller.Url = Mock.Of<IUrlHelper>(u =>
            u.Action(It.IsAny<UrlActionContext>()) == "/Review/Changed/Index?pageNumber=1&recordsPerPage=10");

        var model = new ChangedQualificationsViewModel
        {
            SelectedQualificationIds = new List<Guid> { qualificationId },
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = Guid.NewGuid(),
                Comment = "bulk comment"
            }
        };

        var query = new QualificationQuery { PageNumber = 1, RecordsPerPage = 10 };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkUpdateQualificationsErrorDto>
                    {
                    new()
                    {
                        QualificationId = qualificationId,
                        Qan = "12345678",
                        Title = "Unknown failure qualification",
                        ErrorType = (BulkUpdateQualificationsErrorType)999
                    }
                    }
                }
            });

        await _controller.ApplyBulkAction(model, query);

        var json = Assert.IsType<string>(_controller.TempData[BulkActionQualifications.Errors]);
        var errorModel = JsonSerializer.Deserialize<QualificationBulkActionErrorModel>(json);

        Assert.NotNull(errorModel);
        Assert.Single(errorModel.Failed);
        Assert.Equal("Unknown error.", errorModel.Failed.Single().FailureReason);
    }

    [Fact]
    public void BulkQualificationError_ReturnsViewWithDeserializedModel_WhenTempDataExists()
    {
        var errorModel = new QualificationBulkActionErrorModel
        {
            Failed = new List<QualificationBulkActionErrorItemViewModel>
        {
            new()
            {
                QualificationId = Guid.NewGuid(),
                Qan = "12345678",
                Title = "Test qualification",
                FailureReason = "Qualification not found."
            }
        },
            BackLinkText = "Go back to Changed qualifications",
            BackLinkUrl = "/Review/Changed/Index?pageNumber=1&recordsPerPage=10"
        };

        _controller.TempData[BulkActionQualifications.Errors] = JsonSerializer.Serialize(errorModel);

        var result = _controller.BulkQualificationError();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationBulkActionErrorModel>(viewResult.Model);

        Assert.Single(model.Failed);
        Assert.Equal("12345678", model.Failed.Single().Qan);
        Assert.Equal("Test qualification", model.Failed.Single().Title);
        Assert.Equal("Qualification not found.", model.Failed.Single().FailureReason);
        Assert.Equal("Go back to Changed qualifications", model.BackLinkText);
        Assert.Equal("/Review/Changed/Index?pageNumber=1&recordsPerPage=10", model.BackLinkUrl);
    }

    [Fact]
    public void BulkQualificationError_ReturnsEmptyModel_WhenTempDataMissing()
    {
        var result = _controller.BulkQualificationError();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationBulkActionErrorModel>(viewResult.Model);

        Assert.NotNull(model);
        Assert.Empty(model.Failed);
        Assert.Equal(string.Empty, model.BackLinkText);
        Assert.Equal(string.Empty, model.BackLinkUrl);
    }

    [Fact]
    public void BulkQualificationError_ReturnsEmptyModel_WhenTempDataEmpty()
    {
        _controller.TempData[BulkActionQualifications.Errors] = string.Empty;

        var result = _controller.BulkQualificationError();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationBulkActionErrorModel>(viewResult.Model);

        Assert.NotNull(model);
        Assert.Empty(model.Failed);
        Assert.Equal(string.Empty, model.BackLinkText);
        Assert.Equal(string.Empty, model.BackLinkUrl);
    }

    [Fact]
    public async Task ApplyBulkAction_MapsStatusUpdateFailedErrorReason_WhenErrorsExist()
    {
        var qualificationId = Guid.NewGuid();

        _controller.Url = Mock.Of<IUrlHelper>(u =>
            u.Action(It.IsAny<UrlActionContext>()) == "/Review/Changed/Index?pageNumber=1&recordsPerPage=10");

        var model = new ChangedQualificationsViewModel
        {
            SelectedQualificationIds = new List<Guid> { qualificationId },
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = Guid.NewGuid(),
                Comment = "bulk comment"
            }
        };

        var query = new QualificationQuery { PageNumber = 1, RecordsPerPage = 10 };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkUpdateQualificationsErrorDto>
                    {
                    new()
                    {
                        QualificationId = qualificationId,
                        Qan = "12345678",
                        Title = "Status failure qualification",
                        ErrorType = BulkUpdateQualificationsErrorType.StatusUpdateFailed
                    }
                    }
                }
            });

        await _controller.ApplyBulkAction(model, query);

        var json = Assert.IsType<string>(_controller.TempData[BulkActionQualifications.Errors]);
        var errorModel = JsonSerializer.Deserialize<QualificationBulkActionErrorModel>(json);

        Assert.NotNull(errorModel);
        Assert.Single(errorModel.Failed);
        Assert.Equal("Status update failed.", errorModel.Failed.Single().FailureReason);
    }
}
