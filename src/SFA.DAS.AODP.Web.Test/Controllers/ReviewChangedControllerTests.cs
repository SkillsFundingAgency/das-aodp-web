using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Diagnostics.CodeAnalysis;
using static SFA.DAS.AODP.Application.Queries.Qualifications.GetProcessStatusesQueryResponse;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class ReviewChangedControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<ChangedController>> _loggerMock;
    private readonly Mock<IUserHelperService> _userHelper;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ChangedController _controller;
    private readonly IOptions<AodpConfiguration> _aodpOptions = Options.Create(new AodpConfiguration
    {
        FindRegulatedQualificationUrl = "https://find-a-qualification.services.ofqual.gov.uk/qualifications/"
    });

    public ReviewChangedControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<ChangedController>>>();
        _loggerMock = _fixture.Freeze<Mock<ILogger<ChangedController>>>();
        _userHelper = _fixture.Freeze<Mock<IUserHelperService>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();

        _controller = new ChangedController(_loggerMock.Object, _aodpOptions, _mediatorMock.Object, _userHelper.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_Empty()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture.CreateMany<ChangedQualification>(2).ToList();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        var processResponse = _fixture.Create<BaseMediatrResponse<GetProcessStatusesQueryResponse>>();
        processResponse.Success = true;
        processResponse.Value.ProcessStatuses = _fixture.CreateMany<ProcessStatus>(2).ToList();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), default))
                     .ReturnsAsync(processResponse);

        // Act
        var result = await _controller.Index(processStatusIds: new List<Guid>());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ChangedQualificationsViewModel>(viewResult.ViewData.Model);
        Assert.Equal(_aodpOptions.Value.FindRegulatedQualificationUrl, model.FindRegulatedQualificationUrl);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfNewQualifications()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture.CreateMany<ChangedQualification>(2).ToList();
        queryResponse.Value.TotalRecords = 2;
        queryResponse.Value.Take = 10;
        queryResponse.Value.Skip = 0;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        var processResponse = _fixture.Create<BaseMediatrResponse<GetProcessStatusesQueryResponse>>();
        processResponse.Success = true;
        processResponse.Value.ProcessStatuses = _fixture.CreateMany<ProcessStatus>(2).ToList();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), default))
                     .ReturnsAsync(processResponse);

        // Act
        var result = await _controller.Index(processStatusIds: new List<Guid>(), pageNumber: 1, recordsPerPage: 10);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ChangedQualificationsViewModel>(viewResult.ViewData.Model);
        Assert.Equal(2, model.ChangedQualifications.Count);
        Assert.Equal(queryResponse.Value.Data[0].Subject, model.ChangedQualifications[0].Subject);
        Assert.Equal(queryResponse.Value.Data[0].Status, model.ChangedQualifications[0].Status);
        Assert.Equal(queryResponse.Value.Data[0].AwardingOrganisation, model.ChangedQualifications[0].AwardingOrganisation);
        Assert.Equal(queryResponse.Value.Data[0].Status, model.ChangedQualifications[0].Status);
        Assert.Equal(_aodpOptions.Value.FindRegulatedQualificationUrl, model.FindRegulatedQualificationUrl);
    }

    [Fact]
    public async Task Index_ReturnsNotFound_WhenQueryFails()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>();
        queryResponse.Success = false;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Index(processStatusIds: new List<Guid>(), pageNumber: 1, recordsPerPage: 10);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Home/Error", redirect.Url);
    }

    [Fact(Skip = "This test is being ignored for now.")]
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
        var model = Assert.IsAssignableFrom<ChangedQualificationDetailsViewModel>(viewResult.ViewData.Model);
    }

    [Fact(Skip = "This test is being ignored for now.")]
    [ExcludeFromCodeCoverage]
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
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture.CreateMany<ChangedQualification>(2).ToList();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Clear(recordsPerPage: 10);

        // Assert
        var viewResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", viewResult.ActionName);
    }

    [Fact]
    public async Task ChangePage()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture.CreateMany<ChangedQualification>(2).ToList();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), default))
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
        var viewModel = _fixture.Create<ChangedQualificationsViewModel>();

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
