using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Azure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Routes;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Models.FormBuilder.Form;
using SFA.DAS.AODP.Web.Models.FormBuilder.Page;
using SFA.DAS.AODP.Web.Models.FormBuilder.Question;
using SFA.DAS.AODP.Web.Models.FormBuilder.Routing;
using System.Reflection.Metadata;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class QuestionsControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<QuestionsController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IOptions<FormBuilderSettings>> _formBuilderSettingsMock;
    private readonly QuestionsController _controller;

    public QuestionsControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<QuestionsController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _formBuilderSettingsMock = _fixture.Freeze<Mock<IOptions<FormBuilderSettings>>>();
        _controller = new QuestionsController(_mediatorMock.Object, _loggerMock.Object, _formBuilderSettingsMock.Object);
        _fixture.Customizations.Add(new DateOnlySpecimenBuilder());
    }

    public class DateOnlySpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type type && type == typeof(DateOnly))
            {
                return new DateOnly(2023, 1, 1); // a valid date
            }

            return new NoSpecimen();
        }
    }


    [Fact]
    public async Task Delete_Get_ReturnsViewModel()
    {
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var routesResponse = new BaseMediatrResponse<GetRoutingInformationForQuestionQueryResponse>()
        {
            Success = true,
            Value = _fixture.Create<GetRoutingInformationForQuestionQueryResponse>()
        };


        var queryResponse = new BaseMediatrResponse<GetQuestionByIdQueryResponse>()
        {
            Success = true,
            Value = _fixture.Create<GetQuestionByIdQueryResponse>()
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetRoutingInformationForQuestionQuery>(), default))
                     .ReturnsAsync(routesResponse);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
                     .ReturnsAsync(queryResponse);
        // Act
        var result = await _controller.Delete(formVersionId, sectionId, pageId, questionId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<DeleteQuestionViewModel>(viewResult.ViewData.Model);
        Assert.Equal(formVersionId, model.FormVersionId);
        Assert.Equal(sectionId, model.SectionId);
        Assert.Equal(pageId, model.PageId);
        Assert.Equal(questionId, model.QuestionId);
        Assert.Equal(routesResponse.Value.Routes.Count, model.Routes.Count);
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

        var model = new DeleteQuestionViewModel()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId,
            PageId = pageId,
            QuestionId = questionId
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<DeleteQuestionCommandResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteQuestionCommand>(), default))
                     .ReturnsAsync(queryResponse);

        _controller.TempData = new Mock<ITempDataDictionary>().Object;

        // Act
        var result = await _controller.Delete(model.FormVersionId, model.SectionId, model.PageId, model.QuestionId, model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Edit", redirect.ActionName);
        Assert.Equal("Pages", redirect.ControllerName);
    }

    [Fact]
    public async Task Delete_Post_ReturnsViewModelOnError()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var model = new DeleteQuestionViewModel()
        {
            FormVersionId = formVersionId,
            SectionId = sectionId,
            PageId = pageId,
            QuestionId = questionId
        };

        var queryResponse = _fixture.Create<BaseMediatrResponse<DeleteQuestionCommandResponse>>();
        queryResponse.Success = false;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteQuestionCommand>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Delete(model.FormVersionId, model.SectionId, model.PageId, model.QuestionId, model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var routeModel = Assert.IsAssignableFrom<DeleteQuestionViewModel>(viewResult.ViewData.Model);
        Assert.Equal(formVersionId, routeModel.FormVersionId);
        Assert.Equal(sectionId, routeModel.SectionId);
        Assert.Equal(pageId, routeModel.PageId);
        Assert.Equal(questionId, routeModel.QuestionId);
    }

}
