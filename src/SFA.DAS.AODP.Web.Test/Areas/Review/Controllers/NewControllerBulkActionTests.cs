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
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.BulkActions;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Security.Claims;
using System.Text.Json;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class NewControllerBulkActionTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<NewController>> _loggerMock;
    private readonly Mock<IUserHelperService> _userHelperMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly IOptions<AodpConfiguration> _aodpOptions;
    private readonly NewController _controller;
    private readonly Mock<IUrlHelper> _urlHelperMock;

    public NewControllerBulkActionTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        _loggerMock = _fixture.Freeze<Mock<ILogger<NewController>>>();
        _userHelperMock = _fixture.Freeze<Mock<IUserHelperService>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();

        _aodpOptions = Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = "https://find-a-qualification.services.ofqual.gov.uk/qualifications/"
        });

        _controller = new NewController(
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
            .Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseMediatrResponse<GetNewQualificationsQueryResponse>
            {
                Success = true,
                Value = new GetNewQualificationsQueryResponse
                {
                    Data = new List<NewQualification>(),
                    TotalRecords = 0,
                    Skip = 0,
                    Take = 10
                }
            });

        _urlHelperMock = new Mock<IUrlHelper>();

        _urlHelperMock
            .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
            .Returns("/Review/New/Index?pageNumber=1&recordsPerPage=10");

        _controller.Url = _urlHelperMock.Object;
    }

    [Fact]
    public async Task ApplyBulkAction_ReturnsIndexView_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("BulkAction", "Invalid");

        var model = new NewQualificationsViewModel
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

        var viewModel = Assert.IsType<NewQualificationsViewModel>(viewResult.Model);
        Assert.NotNull(viewModel);

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<BulkUpdateQualificationStatusCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToIndex_AndSetsSuccessFlag_WhenNoErrors()
    {
        var selectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var processStatusId = Guid.NewGuid();

        var model = new NewQualificationsViewModel
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
        Assert.Equal(nameof(NewController.Index), redirectResult.ActionName);

        Assert.True((bool)_controller.TempData[BulkActionQualifications.SuccessKey]!);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToBulkQualificationError_WhenErrorsExist()
    {
        var id = Guid.NewGuid();

        var model = new NewQualificationsViewModel
        {
            SelectedQualificationIds = new List<Guid> { id },
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
                            QualificationId = id,
                            Qan = "12345678",
                            Title = "Test qualification",
                            ErrorType = BulkUpdateQualificationsErrorType.Missing
                        }
                    }
                }
            });

        var result = await _controller.ApplyBulkAction(model, query);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(NewController.BulkQualificationError), redirectResult.ActionName);
    }

    [Fact]
    public async Task ApplyBulkAction_RedirectsToHomeError_WhenExceptionThrown()
    {
        var model = new NewQualificationsViewModel
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

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirectResult.Url);
    }

    [Fact]
    public void BulkQualificationError_ReturnsEmptyModel_WhenTempDataMissing()
    {
        var result = _controller.BulkQualificationError();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QualificationBulkActionErrorModel>(viewResult.Model);

        Assert.NotNull(model);
        Assert.Empty(model.Failed);
    }
}