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
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.BulkActions;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Security.Claims;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class ChangedControllerBulkActionTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<ChangedController>> _loggerMock;
    private readonly Mock<IUserHelperService> _userHelperMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly IOptions<AodpConfiguration> _aodpOptions;
    private readonly ChangedController _controller;

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
    }

    [Fact]
    public async Task ApplyBulkAction_ReturnsIndexView_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("BulkAction", "Invalid");

        var model = _fixture.Create<ChangedQualificationsViewModel>();
        var query = new QualificationQuery { PageNumber = 1, RecordsPerPage = 10 };

        var result = await _controller.ApplyBulkAction(model, query);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        Assert.IsAssignableFrom<ChangedQualificationsViewModel>(view.Model);

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToIndex_WhenNoErrors()
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

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        Assert.True((bool)_controller.TempData[BulkActionQualifications.SuccessKey]!);
    }

    [Fact]
    public async Task ApplyBulkAction_ReturnsIndexView_WhenErrorsExist()
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

        var query = new QualificationQuery { PageNumber = 1, RecordsPerPage = 10 };

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
        Assert.Equal("Index", view.ViewName);

        var vm = Assert.IsAssignableFrom<ChangedQualificationsViewModel>(view.Model);
        Assert.NotNull(vm.BulkUpdateResult);

        Assert.False(_controller.TempData.ContainsKey(BulkActionQualifications.SuccessKey));
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToHomeError_WhenMediatorReturnsFailure()
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

        var query = new QualificationQuery { PageNumber = 1, RecordsPerPage = 10 };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = false,
                ErrorMessage = "API failed"
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
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

        var query = new QualificationQuery { PageNumber = 1, RecordsPerPage = 10 };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        var result = await _controller.ApplyBulkAction(model, query);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact]
    public async Task ApplyBulkAction_SetsUserDisplayName()
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

        var query = new QualificationQuery { PageNumber = 1, RecordsPerPage = 10 };

        _controller.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "bob") }));

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>
            {
                Success = true,
                Value = new BulkUpdateQualificationStatusCommandResponse { ErrorCount = 0 }
            });

        await _controller.ApplyBulkAction(model, query);

        _mediatorMock.Verify(m => m.Send(
            It.Is<BulkUpdateQualificationStatusCommand>(cmd => cmd.UserDisplayName == "bob"),
            It.IsAny<CancellationToken>()),
        Times.Once);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsWithRouteValues()
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
                Value = new BulkUpdateQualificationStatusCommandResponse { ErrorCount = 0 }
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(query.PageNumber, redirect.RouteValues!["pageNumber"]);
        Assert.Equal(query.RecordsPerPage, redirect.RouteValues["recordsPerPage"]);
        Assert.Equal(query.Organisation, redirect.RouteValues["organisation"]);
        Assert.Equal(query.Name, redirect.RouteValues["name"]);
        Assert.Equal(query.Qan, redirect.RouteValues["qan"]);
    }
}
