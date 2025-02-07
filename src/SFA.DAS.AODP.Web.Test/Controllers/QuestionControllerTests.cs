using AutoFixture;
using Azure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;
using SFA.DAS.AODP.Web.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Models.FormBuilder.Question;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class QuestionsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly QuestionsController _controller;
    private readonly Fixture _fixture = new();

    public QuestionsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new QuestionsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Delete_WhenQuestionNotFound_ReturnsNotFound()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var mockResponse = new BaseMediatrResponse<GetQuestionByIdQueryResponse>();
        mockResponse.Success = false;
        mockResponse.Value = null;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.Delete(formVersionId, sectionId, pageId, questionId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_WhenQuestionFound_ReturnsView()
    {
        // Arrange
        var formVersionId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQuestionByIdQuery>(), default))
            .ReturnsAsync(new BaseMediatrResponse<GetQuestionByIdQueryResponse>());

        // Act
        var result = await _controller.Delete(formVersionId, sectionId, pageId, questionId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<DeleteQuestionViewModel>(viewResult.Model);
    }
}