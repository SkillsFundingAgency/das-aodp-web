using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Routes;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Models.FormBuilder.Routing;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class RoutesControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<RoutesController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly RoutesController _controller;

    public RoutesControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<RoutesController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _controller = new RoutesController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Delete_Get_ReturnsViewModel()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        // Act
        var result = await _controller.Delete(formVersionId, sectionId, pageId, questionId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<DeleteRouteViewModel>(viewResult.ViewData.Model);
        Assert.Equal(formVersionId, model.FormVersionId);
        Assert.Equal(sectionId, model.SectionId);
        Assert.Equal(pageId, model.PageId);
        Assert.Equal(questionId, model.QuestionId);
    }

    [Fact]
    public async Task Delete_Post_ReturnsRedirectToListPage()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var model = new DeleteRouteViewModel()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId,
            PageId = pageId,
            QuestionId = questionId
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<EmptyResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteRouteCommand>(), default))
                     .ReturnsAsync(queryResponse);

        _controller.TempData = new Mock<ITempDataDictionary>().Object;

        // Act
        var result = await _controller.Delete(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("List", redirect.ActionName);
    }

    [Fact]
    public async Task Delete_Post_ReturnsViewModelOnError()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var model = new DeleteRouteViewModel()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId,
            PageId = pageId,
            QuestionId = questionId
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<EmptyResponse>>();
        queryResponse.Success = false;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteRouteCommand>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Delete(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<DeleteRouteViewModel>(viewResult.ViewData.Model);
        Assert.Equal(formVersionId, routeModel.FormVersionId);
        Assert.Equal(sectionId, routeModel.SectionId);
        Assert.Equal(pageId, routeModel.PageId);
        Assert.Equal(questionId, routeModel.QuestionId);
    }

}
