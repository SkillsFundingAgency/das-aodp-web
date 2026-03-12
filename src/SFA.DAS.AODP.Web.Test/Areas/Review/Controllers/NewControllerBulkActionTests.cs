using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Qualifications;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.BulkActions;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Security.Claims;
using Xunit;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class NewControllerBulkActionTests
{
    private const string FindRegulatedQualificationUrl =
        "https://find-a-qualification.services.ofqual.gov.uk/qualifications/";

    private const string DefaultUserName = "TestUser";
    private const string AlternateUserName = "bob";

    private const string BulkComment = "bulk comment";
    private const string SearchOrganisation = "Org";
    private const string SearchName = "Qual";
    private const string SearchQan = "123";

    private const string DecisionRequiredStatus = "Decision Required";

    private const int DefaultPageNumber = 1;
    private const int DefaultRecordsPerPage = 10;
    private const int AlternatePageNumber = 2;
    private const int AlternateRecordsPerPage = 20;

    private readonly IFixture _fixture;
    private readonly Mock<ILogger<NewController>> _loggerMock;
    private readonly Mock<IUserHelperService> _userHelperMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly IOptions<AodpConfiguration> _aodpOptions;
    private readonly NewController _controller;

    public NewControllerBulkActionTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _loggerMock = _fixture.Freeze<Mock<ILogger<NewController>>>();
        _userHelperMock = _fixture.Freeze<Mock<IUserHelperService>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();

        _aodpOptions = Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = FindRegulatedQualificationUrl
        });

        _controller = new NewController(
            _loggerMock.Object,
            _aodpOptions,
            _mediatorMock.Object,
            _userHelperMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, DefaultUserName) },
                "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
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
                        new() { Id = Guid.NewGuid(), Name = DecisionRequiredStatus }
                    }
                }
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetNewQualificationsQueryResponse>
            {
                Success = true,
                Value = new GetNewQualificationsQueryResponse
                {
                    Data = new List<NewQualification>(),
                    TotalRecords = 0,
                    Skip = 0,
                    Take = DefaultRecordsPerPage
                }
            });
    }

    private static QualificationQuery CreateQualificationQuery(
        int pageNumber = DefaultPageNumber,
        int recordsPerPage = DefaultRecordsPerPage,
        string? organisation = null,
        string? name = null,
        string? qan = null)
    {
        return new QualificationQuery
        {
            PageNumber = pageNumber,
            RecordsPerPage = recordsPerPage,
            Organisation = organisation,
            Name = name,
            Qan = qan
        };
    }

    private static NewQualificationsViewModel CreateBulkActionModel(
        List<Guid>? selectedIds = null,
        Guid? processStatusId = null,
        string comment = BulkComment)
    {
        return new NewQualificationsViewModel
        {
            SelectedQualificationIds = selectedIds ?? new List<Guid> { Guid.NewGuid() },
            BulkAction = new QualificationsBulkActionPageViewModel.QualificationsBulkActionInputViewModel
            {
                ProcessStatusId = processStatusId ?? Guid.NewGuid(),
                Comment = comment
            }
        };
    }

    [Fact]
    public async Task ApplyBulkAction_ReturnsIndexView_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("BulkAction", "Invalid");

        var selectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var processStatusId = Guid.NewGuid();

        var model = CreateBulkActionModel(selectedIds, processStatusId);
        var query = CreateQualificationQuery();

        var result = await _controller.ApplyBulkAction(model, query);

        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsAssignableFrom<NewQualificationsViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.Equal("Index", view.ViewName);
            Assert.Equal(selectedIds, viewModel.SelectedQualificationIds);
            Assert.NotNull(viewModel.BulkAction);
            Assert.Equal(processStatusId, viewModel.BulkAction.ProcessStatusId);
            Assert.Equal(BulkComment, viewModel.BulkAction.Comment);
            Assert.Equal(FindRegulatedQualificationUrl, viewModel.FindRegulatedQualificationUrl);
            Assert.NotNull(viewModel.ProcessStatuses);
        });

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToIndex_WhenNoErrors()
    {
        var selectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var processStatusId = Guid.NewGuid();

        var model = CreateBulkActionModel(selectedIds, processStatusId);

        var query = CreateQualificationQuery(
            pageNumber: AlternatePageNumber,
            recordsPerPage: AlternateRecordsPerPage,
            organisation: SearchOrganisation,
            name: SearchName,
            qan: SearchQan);

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

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(NewController.Index), redirect.ActionName);
            Assert.Equal(AlternatePageNumber, redirect.RouteValues!["pageNumber"]);
            Assert.Equal(AlternateRecordsPerPage, redirect.RouteValues["recordsPerPage"]);
            Assert.Equal(SearchOrganisation, redirect.RouteValues["organisation"]);
            Assert.Equal(SearchName, redirect.RouteValues["name"]);
            Assert.Equal(SearchQan, redirect.RouteValues["qan"]);
            Assert.True((bool)_controller.TempData[BulkActionQualifications.SuccessKey]!);
        });

        _mediatorMock.Verify(m => m.Send(
            It.Is<BulkUpdateQualificationStatusCommand>(cmd =>
                cmd.QualificationIds.SequenceEqual(selectedIds) &&
                cmd.ProcessStatusId == processStatusId &&
                cmd.Comment == BulkComment &&
                cmd.UserDisplayName == DefaultUserName),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApplyBulkAction_ReturnsIndexView_WhenErrorsExist()
    {
        var selectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var processStatusId = Guid.NewGuid();

        var model = CreateBulkActionModel(selectedIds, processStatusId);
        var query = CreateQualificationQuery();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse
                {
                    ErrorCount = 1,
                    Errors = new List<BulkQualificationErrorDto>
                    {
                        new BulkQualificationErrorDto
                        {
                            QualificationId = selectedIds[0],
                            Title = "Missing",
                            ErrorType = BulkQualificationErrorType.Missing
                        }
                    }
                }
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsAssignableFrom<NewQualificationsViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.Equal("Index", view.ViewName);
            Assert.NotNull(viewModel.BulkUpdateResult);
            Assert.Equal(selectedIds, viewModel.SelectedQualificationIds);
            Assert.NotNull(viewModel.BulkAction);
            Assert.Equal(processStatusId, viewModel.BulkAction.ProcessStatusId);
            Assert.Equal(BulkComment, viewModel.BulkAction.Comment);
            Assert.False(_controller.TempData.ContainsKey(BulkActionQualifications.SuccessKey));
        });
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToHomeError_WhenMediatorReturnsFailure()
    {
        var model = CreateBulkActionModel();
        var query = CreateQualificationQuery();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = false,
                ErrorMessage = "API failed"
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var redirect = Assert.IsType<RedirectResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal("/Home/Error", redirect.Url);
        });
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToHomeError_WhenExceptionThrown()
    {
        var model = CreateBulkActionModel();
        var query = CreateQualificationQuery();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await _controller.ApplyBulkAction(model, query);

        var redirect = Assert.IsType<RedirectResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal("/Home/Error", redirect.Url);
        });
    }

    [Fact]
    public async Task ApplyBulkAction_SetsUserDisplayName()
    {
        var model = CreateBulkActionModel();
        var query = CreateQualificationQuery();

        _controller.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, AlternateUserName) }));

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
                cmd.UserDisplayName == AlternateUserName),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsWithRouteValues()
    {
        var model = CreateBulkActionModel();

        var query = CreateQualificationQuery(
            pageNumber: AlternatePageNumber,
            recordsPerPage: AlternateRecordsPerPage,
            organisation: SearchOrganisation,
            name: SearchName,
            qan: SearchQan);

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

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Multiple(() =>
        {
            Assert.Equal(AlternatePageNumber, redirect.RouteValues!["pageNumber"]);
            Assert.Equal(AlternateRecordsPerPage, redirect.RouteValues["recordsPerPage"]);
            Assert.Equal(SearchOrganisation, redirect.RouteValues["organisation"]);
            Assert.Equal(SearchName, redirect.RouteValues["name"]);
            Assert.Equal(SearchQan, redirect.RouteValues["qan"]);
        });
    }

    [Fact]
    public async Task ApplyBulkAction_InvalidModelState_PreservesPostedSelectionAndBulkAction()
    {
        _controller.ModelState.AddModelError("BulkAction", "Invalid");

        var selectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var processStatusId = Guid.NewGuid();

        var model = CreateBulkActionModel(selectedIds, processStatusId, BulkComment);
        var query = CreateQualificationQuery(
            pageNumber: AlternatePageNumber,
            recordsPerPage: AlternateRecordsPerPage,
            organisation: SearchOrganisation,
            name: SearchName,
            qan: SearchQan);

        var result = await _controller.ApplyBulkAction(model, query);

        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsAssignableFrom<NewQualificationsViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.Equal("Index", view.ViewName);
            Assert.Equal(selectedIds, viewModel.SelectedQualificationIds);
            Assert.NotNull(viewModel.BulkAction);
            Assert.Equal(processStatusId, viewModel.BulkAction.ProcessStatusId);
            Assert.Equal(BulkComment, viewModel.BulkAction.Comment);
            Assert.NotNull(viewModel.Filter);
            Assert.Equal(SearchOrganisation, viewModel.Filter.Organisation);
            Assert.Equal(SearchName, viewModel.Filter.QualificationName);
            Assert.Equal(SearchQan, viewModel.Filter.QAN);
        });
    }

    [Fact]
    public async Task ApplyBulkAction_ErrorResult_PreservesPostedSelectionAndBulkAction()
    {
        var selectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var processStatusId = Guid.NewGuid();

        var model = CreateBulkActionModel(selectedIds, processStatusId, BulkComment);
        var query = CreateQualificationQuery(
            pageNumber: AlternatePageNumber,
            recordsPerPage: AlternateRecordsPerPage,
            organisation: SearchOrganisation,
            name: SearchName,
            qan: SearchQan);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse
                {
                    ErrorCount = 2,
                    Errors = new List<BulkQualificationErrorDto>
                    {
                        new BulkQualificationErrorDto
                        {
                            QualificationId = selectedIds[0],
                            Title = "Missing",
                            ErrorType = BulkQualificationErrorType.Missing
                        },
                        new BulkQualificationErrorDto
                        {
                            QualificationId = selectedIds[1],
                            Title = "Locked",
                            ErrorType = BulkQualificationErrorType.StatusUpdateFailed
                        }
                    }
                }
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsAssignableFrom<NewQualificationsViewModel>(view.Model);

        Assert.Multiple(() =>
        {
            Assert.Equal("Index", view.ViewName);
            Assert.NotNull(viewModel.BulkUpdateResult);
            Assert.Equal(selectedIds, viewModel.SelectedQualificationIds);
            Assert.NotNull(viewModel.BulkAction);
            Assert.Equal(processStatusId, viewModel.BulkAction.ProcessStatusId);
            Assert.Equal(BulkComment, viewModel.BulkAction.Comment);
            Assert.NotNull(viewModel.Filter);
            Assert.Equal(SearchOrganisation, viewModel.Filter.Organisation);
            Assert.Equal(SearchName, viewModel.Filter.QualificationName);
            Assert.Equal(SearchQan, viewModel.Filter.QAN);
        });
    }
}