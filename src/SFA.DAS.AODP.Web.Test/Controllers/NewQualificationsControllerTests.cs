using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Application.Queries.Test;
using SFA.DAS.AODP.Web.Controllers;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class NewQualificationsControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<NewQualificationsController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly NewQualificationsController _controller;

    public NewQualificationsControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<NewQualificationsController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _controller = _fixture.Create<NewQualificationsController>();
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfNewQualifications()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetNewQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Value.NewQualifications = _fixture.CreateMany<NewQualification>(2).ToList();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<NewQualificationsViewModel>>(viewResult.ViewData.Model);
        Assert.Equal(2, model.Count);
        Assert.Equal(queryResponse.Value.Value.NewQualifications[0].Title, model[0].Title);
        Assert.Equal(queryResponse.Value.Value.NewQualifications[0].Reference, model[0].Reference);
        Assert.Equal(queryResponse.Value.Value.NewQualifications[0].AwardingOrganisation, model[0].AwardingOrganisation);
        Assert.Equal(queryResponse.Value.Value.NewQualifications[0].Status, model[0].Status);
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
        var result = await _controller.Index();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Error", notFoundResult.Value);
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
        Assert.Equal(queryResponse.Value.Value.Id, model.Id);
        Assert.Equal(queryResponse.Value.Value.Status, model.Status);
    }

    [Fact]
    public async Task QualificationDetails_ReturnsNotFound_WhenQueryFails()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQualificationDetailsQueryResponse>>();
        queryResponse.Success = false;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.QualificationDetails("Ref123");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
