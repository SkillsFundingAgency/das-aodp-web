using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Models.FormBuilder.Page;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class PagesControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<PagesController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly PagesController _controller;

    public PagesControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<PagesController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _controller = new PagesController(_mediatorMock.Object, _loggerMock.Object);
    }


    [Fact]
    public async Task Delete_Get_ReturnsViewModel()
    {
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var queryResponse = new BaseMediatrResponse<GetPageByIdQueryResponse>()
        {
            Success = true,
            Value = _fixture.Create<GetPageByIdQueryResponse>()
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPageByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);
        // Act
        var result = await _controller.Delete(formVersionId, sectionId, pageId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<DeletePageViewModel>(viewResult.ViewData.Model);
        Assert.Equal(formVersionId, model.FormVersionId);
        Assert.Equal(sectionId, model.SectionId);
        Assert.Equal(pageId, model.PageId);
        Assert.Equal(queryResponse.Value.HasAssociatedRoutes, model.HasAssociatedRoutes);
        Assert.Equal(queryResponse.Value.Title, model.Title);
    }

    [Fact]
    public async Task Delete_Post_ReturnsRedirectToListPage()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var model = new DeletePageViewModel()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId,
            PageId = pageId,
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<DeletePageCommandResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeletePageCommand>(), default))
                     .ReturnsAsync(queryResponse);

        _controller.TempData = new Mock<ITempDataDictionary>().Object;

        // Act
        var result = await _controller.DeleteConfirmed(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Edit", redirect.ActionName);
        Assert.Equal("Sections", redirect.ControllerName);
    }

    [Fact]
    public async Task Delete_Post_ReturnsViewModelOnError()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var model = new DeletePageViewModel()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId,
            PageId = pageId,
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<DeletePageCommandResponse>>();
        queryResponse.Success = false;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeletePageCommand>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.DeleteConfirmed(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<DeletePageViewModel>(viewResult.ViewData.Model);
        Assert.Equal(formVersionId, routeModel.FormVersionId);
        Assert.Equal(sectionId, routeModel.SectionId);
        Assert.Equal(pageId, routeModel.PageId);
    }

}
