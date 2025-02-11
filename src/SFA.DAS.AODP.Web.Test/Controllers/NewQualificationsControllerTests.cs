using Moq;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Controllers;
using SFA.DAS.AODP.Web.Models.Qualifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using SFA.DAS.AODP.Application.Queries.Test;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class NewQualificationsControllerTests
{
    private readonly Mock<ILogger<NewQualificationsController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly NewQualificationsController _controller;

    public NewQualificationsControllerTests()
    {
        _loggerMock = new Mock<ILogger<NewQualificationsController>>();
        _mediatorMock = new Mock<IMediator>();
        _controller = new NewQualificationsController(_loggerMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfNewQualifications()
    {
        // Arrange
        var queryResponse = new GetNewQualificationsQueryResponse
        {
            Success = true,
            NewQualifications = new List<NewQualification>
            {
                new NewQualification { Id = 1, Title = "Qualification 1", Reference = "Ref 1", AwardingOrganisation = "AO 1", Status = "Status 1" },
                new NewQualification { Id = 2, Title = "Qualification 2", Reference = "Ref 2", AwardingOrganisation = "AO 2", Status = "Status 2" }
            }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<NewQualificationsViewModel>>(viewResult.ViewData.Model);
        Assert.Equal(2, model.Count);
        Assert.Equal("Qualification 1", model[0].Title);
        Assert.Equal("Ref 1", model[0].Reference);
        Assert.Equal("AO 1", model[0].AwardingOrganisation);
        Assert.Equal("Status 1", model[0].Status);
    }

    [Fact]
    public async Task Index_ReturnsNotFound_WhenQueryFails()
    {
        // Arrange
        var queryResponse = new GetNewQualificationsQueryResponse { Success = false };
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
        var queryResponse = new GetQualificationDetailsQueryResponse
        {
            Success = true,
            Id = 1,
            Status = "Active",
            Priority = "High",
            Changes = "None",
            QualificationReference = "Ref123",
            AwardingOrganisation = "Org1",
            Title = "Qualification 1",
            QualificationType = "Type1",
            Level = "Level1",
            ProposedChanges = "None",
            AgeGroup = "18+",
            Category = "Category1",
            Subject = "Subject1",
            SectorSubjectArea = "Area1",
            Comments = "No comments"
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.QualificationDetails("Ref123");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<QualificationDetailsViewModel>(viewResult.ViewData.Model);
        Assert.Equal(1, model.Id);
        Assert.Equal("Active", model.Status);
    }

    [Fact]
    public async Task QualificationDetails_ReturnsNotFound_WhenQueryFails()
    {
        // Arrange
        var queryResponse = new GetQualificationDetailsQueryResponse { Success = false };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.QualificationDetails("Ref123");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

}