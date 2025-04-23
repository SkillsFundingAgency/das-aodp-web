using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Routes;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
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


    [Fact]
    public async Task ChooseSection_Get_ReturnsViewModel()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetAvailableSectionsAndPagesForRoutingQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAvailableSectionsAndPagesForRoutingQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.ChooseSection(formVersionId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<CreateRouteChooseSectionViewModel>(viewResult.ViewData.Model);
        Assert.Equal(formVersionId, model.FormVersionId);
        Assert.Equal(queryResponse.Value.Sections.Count, model.Sections.Count);
    }

    [Fact]
    public async Task ChooseSection_Post_ReturnsRedirectToAction()
    {
        // Arrange
        var model = _fixture.Create<CreateRouteChooseSectionViewModel>();

        // Act
        var result = await _controller.ChooseSection(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ChoosePage", redirect.ActionName);
        Assert.Equal(model.FormVersionId, redirect.RouteValues["formVersionId"]);
        Assert.Equal(model.ChosenSectionId, redirect.RouteValues["sectionId"]);
    }

    [Fact]
    public async Task ChooseSection_Post_ReturnsViewModelOnError()
    {
        // Arrange
        var model = _fixture.Create<CreateRouteChooseSectionViewModel>();


        var queryResponse = _fixture.Create<BaseMediatrResponse<GetAvailableSectionsAndPagesForRoutingQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAvailableSectionsAndPagesForRoutingQuery>(), default))
                     .ReturnsAsync(queryResponse);

        _controller.ModelState.AddModelError("", "");

        // Act
        var result = await _controller.ChooseSection(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var responseModel = Assert.IsAssignableFrom<CreateRouteChooseSectionViewModel>(viewResult.ViewData.Model);
        Assert.Equal(queryResponse.Value.Sections.Count, model.Sections.Count);
    }

    [Fact]
    public async Task ChoosePage_Get_ReturnsViewModel()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetAvailableSectionsAndPagesForRoutingQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAvailableSectionsAndPagesForRoutingQuery>(), default))
                     .ReturnsAsync(queryResponse);

        var sectionid = queryResponse.Value.Sections.First().Id;

        // Act
        var result = await _controller.ChoosePage(formVersionId, sectionid);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<CreateRouteChoosePageViewModel>(viewResult.ViewData.Model);
        Assert.Equal(formVersionId, model.FormVersionId);
        Assert.Equal(sectionid, model.ChosenSectionId);
        Assert.Equal(queryResponse.Value.Sections.First().Pages.Count, model.Pages.Count);
    }


    [Fact]
    public async Task ChoosePage_Post_ReturnsRedirectToAction()
    {
        // Arrange
        var model = _fixture.Create<CreateRouteChoosePageViewModel>();

        // Act
        var result = await _controller.ChoosePage(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ChooseQuestion", redirect.ActionName);
        Assert.Equal(redirect.RouteValues["formVersionId"], model.FormVersionId);
        Assert.Equal(redirect.RouteValues["sectionId"], model.ChosenSectionId);
        Assert.Equal(redirect.RouteValues["pageId"], model.ChosenPageId);
    }

    [Fact]
    public async Task ChoosePage_Post_ReturnsViewModelOnError()
    {
        // Arrange
        var model = _fixture.Create<CreateRouteChoosePageViewModel>();


        var queryResponse = _fixture.Create<BaseMediatrResponse<GetAvailableSectionsAndPagesForRoutingQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Sections.First().Id = model.ChosenSectionId;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAvailableSectionsAndPagesForRoutingQuery>(), default))
                     .ReturnsAsync(queryResponse);

        _controller.ModelState.AddModelError("", "");
        // Act
        var result = await _controller.ChoosePage(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var responseModel = Assert.IsAssignableFrom<CreateRouteChoosePageViewModel>(viewResult.ViewData.Model);
        Assert.Equal(responseModel.FormVersionId, model.FormVersionId);
        Assert.Equal(responseModel.ChosenSectionId, model.ChosenSectionId);
        Assert.Equal(queryResponse.Value.Sections.First().Pages.Count, model.Pages.Count);
    }


    [Fact]
    public async Task ChooseQuestion_Get_ReturnsViewModel()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetAvailableQuestionsForRoutingQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAvailableQuestionsForRoutingQuery>(), default))
                     .ReturnsAsync(queryResponse);


        // Act
        var result = await _controller.ChooseQuestion(formVersionId, sectionId, pageId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<CreateRouteChooseQuestionViewModel>(viewResult.ViewData.Model);
        Assert.Equal(formVersionId, model.FormVersionId);
        Assert.Equal(sectionId, model.SectionId);
        Assert.Equal(pageId, model.PageId);

        Assert.Equal(queryResponse.Value.Questions.Count, model.Questions.Count);
    }

    [Fact]
    public async Task ChooseQuestion_Post_ReturnsRedirectToAction()
    {
        // Arrange
        var model = _fixture.Create<CreateRouteChooseQuestionViewModel>();

        // Act
        var result = await _controller.ChooseQuestion(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Configure", redirect.ActionName);
        Assert.Equal(model.FormVersionId, redirect.RouteValues["formVersionId"]);
        Assert.Equal(model.ChosenQuestionId, redirect.RouteValues["questionId"]);
    }


    [Fact]
    public async Task ChooseQuestion_Post_ReturnsViewModelOnError()
    {
        // Arrange
        var model = _fixture.Create<CreateRouteChooseQuestionViewModel>();

        var queryResponse = _fixture.Create<BaseMediatrResponse<GetAvailableQuestionsForRoutingQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAvailableQuestionsForRoutingQuery>(), default))
                     .ReturnsAsync(queryResponse);

        _controller.ModelState.AddModelError("", "");
        // Act
        var result = await _controller.ChooseQuestion(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var responseModel = Assert.IsAssignableFrom<CreateRouteChooseQuestionViewModel>(viewResult.ViewData.Model);
        Assert.Equal(model.FormVersionId, responseModel.FormVersionId);
        Assert.Equal(model.SectionId, responseModel.SectionId);
        Assert.Equal(model.PageId, responseModel.PageId);
        Assert.Equal(queryResponse.Value.Questions.Count, responseModel.Questions.Count);
    }
}
