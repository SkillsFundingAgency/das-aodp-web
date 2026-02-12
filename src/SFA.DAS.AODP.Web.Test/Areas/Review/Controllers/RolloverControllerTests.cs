using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class RolloverControllerTests
{
    private readonly Mock<ILogger<RolloverController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly RolloverController _controller;

    public RolloverControllerTests()
    {
        _loggerMock = new Mock<ILogger<RolloverController>>();
        _mediatorMock = new Mock<IMediator>();
        _controller = new RolloverController(_loggerMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public void Index_Get_ReturnsViewWithModel()
    {
        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("RolloverStart", viewResult.ViewName);
        Assert.IsType<RolloverStartViewModel>(viewResult.Model);
    }

    [Fact]
    public void Index_Post_InvalidModelState_ReturnsStartViewWithModel()
    {
        // Arrange
        _controller.ModelState.AddModelError("SelectedProcess", "required");
        var vm = new RolloverStartViewModel();

        // Act
        var result = _controller.Index(vm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("RolloverStart", viewResult.ViewName);
        Assert.Same(vm, viewResult.Model);
    }

    [Fact]
    public void Index_Post_SelectedProcessInitialSelection_RedirectsToInitialSelection()
    {
        // Arrange
        var vm = new RolloverStartViewModel
        {
            SelectedProcess = RolloverProcess.InitialSelection
        };

        // Act
        var result = _controller.Index(vm);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.InitialSelection), redirect.ActionName);
    }

    [Fact]
    public void Index_Post_SelectedProcessFinalUpload_RedirectsToUploadQualifications()
    {
        // Arrange
        var vm = new RolloverStartViewModel
        {
            SelectedProcess = RolloverProcess.FinalUpload
        };

        // Act
        var result = _controller.Index(vm);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.UploadQualifications), redirect.ActionName);
    }

    [Fact]
    public void InitialSelection_Get_ReturnsViewAndSetsTitle()
    {
        // Act
        var result = _controller.InitialSelection();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Initial selection of qualificaton", viewResult.ViewData["Title"]);
    }

    [Fact]
    public void UploadQualifications_Get_ReturnsViewAndSetsTitle()
    {
        // Act
        var result = _controller.UploadQualifications();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Upload qualifications to RollOver", viewResult.ViewData["Title"]);
    }
}