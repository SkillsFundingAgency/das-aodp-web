﻿using AutoFixture;
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

public class ReviewNewControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<NewController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly NewController _controller;

    public ReviewNewControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<NewController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _controller = new NewController(_loggerMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_Empty()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetNewQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture.CreateMany<NewQualification>(2).ToList();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<NewQualificationsViewModel>(viewResult.ViewData.Model);        
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfNewQualifications()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetNewQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture.CreateMany<NewQualification>(2).ToList();
        queryResponse.Value.TotalRecords = 2;
        queryResponse.Value.Take = 10;
        queryResponse.Value.Skip = 0;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Index(pageNumber: 1, recordsPerPage: 10);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<NewQualificationsViewModel>(viewResult.ViewData.Model);
        Assert.Equal(2, model.NewQualifications.Count);
        Assert.Equal(queryResponse.Value.Data[0].Title, model.NewQualifications[0].Title);
        Assert.Equal(queryResponse.Value.Data[0].Reference, model.NewQualifications[0].Reference);
        Assert.Equal(queryResponse.Value.Data[0].AwardingOrganisation, model.NewQualifications[0].AwardingOrganisation);
        Assert.Equal(queryResponse.Value.Data[0].Status, model.NewQualifications[0].Status);
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
        var result = await _controller.Index(pageNumber: 1, recordsPerPage: 10);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
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
        try
        {
            var result = await _controller.QualificationDetails("Ref123");
            Assert.Fail();
        }
        catch (Exception ex)
        {
            Assert.Equal(queryResponse.ErrorMessage, ex.Message);
        }
    }

    [Fact]
    public async Task QualificationDetails_ReturnsBadRequest_WhenQualificationReferenceIsEmpty()
    {
        // Act
        var result = await _controller.QualificationDetails(string.Empty);

        // Assert
        var badRequestResult = Assert.IsType<RedirectResult>(result);        
    }

    [Fact]
    public async Task Clear_Empty()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetNewQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture.CreateMany<NewQualification>(2).ToList();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Clear(recordsPerPage: 10);

        // Assert
        var viewResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index",viewResult.ActionName);
    }

    [Fact]
    public async Task ChangePage()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetNewQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture.CreateMany<NewQualification>(2).ToList();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNewQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.ChangePage(newPage: 2, recordsPerPage: 10);

        // Assert
        var viewResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", viewResult.ActionName);
    }

    [Fact]
    public async Task Search()
    {
        // Arrange
        var viewModel = _fixture.Create<NewQualificationsViewModel>();       

        // Act
        var result = await _controller.Search(viewModel);

        // Assert
        var viewResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", viewResult.ActionName);
        Assert.Equal(viewModel.PaginationViewModel.RecordsPerPage, viewResult.RouteValues["recordsPerPage"]);
        Assert.Equal(viewModel.Filter.Organisation, viewResult.RouteValues["organisation"]);
        Assert.Equal(viewModel.Filter.QualificationName, viewResult.RouteValues["name"]);
        Assert.Equal(viewModel.Filter.QAN, viewResult.RouteValues["qan"]);
    }
}
