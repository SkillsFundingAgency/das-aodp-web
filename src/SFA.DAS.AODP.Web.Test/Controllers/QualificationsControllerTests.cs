using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class QualificationsControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<QualificationsController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly QualificationsController _controller;

    public QualificationsControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<QualificationsController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _controller = new QualificationsController(_loggerMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfNewQualifications()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetNewQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture.CreateMany<NewQualification>(2).ToList();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        //var result = await _controller.Index("new");

        // Assert
        //var viewResult = Assert.IsType<ViewResult>(result);
        //var model = Assert.IsAssignableFrom<List<NewQualificationViewModel>>(viewResult.ViewData.Model);
        //Assert.Equal(2, model.Count);
        //Assert.Equal(queryResponse.Value.Data[0].Title, model[0].Title);
        //Assert.Equal(queryResponse.Value.Data[0].Reference, model[0].Reference);
        //Assert.Equal(queryResponse.Value.Data[0].AwardingOrganisation, model[0].AwardingOrganisation);
        //Assert.Equal(queryResponse.Value.Data[0].Status, model[0].Status);
    }

    [Fact]
    public async Task Index_ReturnsNotFound_WhenQueryFails()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetNewQualificationsQueryResponse>>();
        queryResponse.Success = false;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        //var result = await _controller.Index("new");

        // Assert
        //var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        //var notFoundValue = notFoundResult.Value?.GetType().GetProperty("message")?.GetValue(notFoundResult.Value, null);
        //Assert.Equal("No new qualifications found", notFoundValue);
    }

    [Fact]
    public async Task Index_ReturnsNotFound_WhenMediatorReturnsNull()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetNewQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value = null;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        //var result = await _controller.Index("new");

        // Assert
        //var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        //var notFoundValue = notFoundResult.Value?.GetType().GetProperty("message")?.GetValue(notFoundResult.Value, null);
        //Assert.Equal("No new qualifications found", notFoundValue);
    }

    [Fact]
    public async Task QualificationDetails_ReturnsViewResult_WithQualificationDetails()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQualificationDetailsQueryResponse>>();
        queryResponse.Success = true;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.QualificationDetails("Ref123");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<QualificationDetailsViewModel>(viewResult.ViewData.Model);
        Assert.Equal(queryResponse.Value.Id, model.Id);
        Assert.Equal(queryResponse.Value.Status, model.Status);
    }

    [Fact]
    public async Task QualificationDetails_ReturnsNotFound_WhenQueryFails()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQualificationDetailsQueryResponse>>();
        queryResponse.Success = false;
        queryResponse.ErrorMessage = "No details found for qualification reference: Ref123";

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.QualificationDetails("Ref123");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task QualificationDetails_ReturnsBadRequest_WhenQualificationReferenceIsEmpty()
    {
        // Act
        var result = await _controller.QualificationDetails(string.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var badRequestValue = badRequestResult.Value?.GetType().GetProperty("message")?.GetValue(badRequestResult.Value, null);
        Assert.Equal("Qualification reference cannot be empty", badRequestValue);
    }
}
