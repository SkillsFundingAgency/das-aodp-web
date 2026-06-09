using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Application.Services;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.AODP.Models.Qualifications;

namespace SFA.DAS.AODP.Web.UnitTests.Controllers;

public class ReviewChangedControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ChangedController _controller;

    public ReviewChangedControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        var loggerMock = _fixture.Freeze<Mock<ILogger<ChangedController>>>();
        var userHelper = _fixture.Freeze<Mock<IUserHelperService>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        var timelineBuilder = _fixture.Freeze<Mock<IQualificationTimelineHistoryBuilder>>();

        _controller = new ChangedController(loggerMock.Object, _mediatorMock.Object, userHelper.Object,
            timelineBuilder.Object);

        userHelper
            .Setup(u => u.GetUserRoles())
            .Returns(new List<string>());

        var processResponse = new BaseMediatrResponse<GetProcessStatusesQueryResponse>
        {
            Success = true,
            Value = new GetProcessStatusesQueryResponse
            {
                ProcessStatuses = new List<ProcessStatus>
                {
                    new() { Id = Guid.NewGuid(), Name = "Decision Required" },
                    new() { Id = Guid.NewGuid(), Name = "No Action Required" }
                }
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(processResponse);

    }

    [Fact]
    public async Task Index_ReturnsViewResult_Empty()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture
            .Build<ChangedQualification>()
            .With(x => x.Status, ProcessStatusLookup.DecisionRequired.Name)
            .CreateMany(2).ToList();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), CancellationToken.None))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Index(new());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<ChangedQualificationsViewModel>(viewResult.ViewData.Model);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithListOfNewQualifications()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture
            .Build<ChangedQualification>()
            .With(x => x.Status, ProcessStatusLookup.DecisionRequired.Name)
            .CreateMany(2).ToList();

        queryResponse.Value.TotalRecords = 2;
        queryResponse.Value.Take = 10;
        queryResponse.Value.Skip = 0;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), CancellationToken.None))
                     .ReturnsAsync(queryResponse);

        var qualificationQuery =
            new QualificationQuery
            {
                PageNumber = 1,
                RecordsPerPage = 10
            };

        // Act
        var result = await _controller.Index(qualificationQuery);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ChangedQualificationsViewModel>(viewResult.ViewData.Model);
        Assert.Equal(2, model.ChangedQualifications.Count);
        Assert.Equal(queryResponse.Value.Data[0].Subject, model.ChangedQualifications[0].Subject);
        Assert.Equal(queryResponse.Value.Data[0].Status, model.ChangedQualifications[0].CurrentProcessStatus.Name);
        Assert.Equal(queryResponse.Value.Data[0].AwardingOrganisation, model.ChangedQualifications[0].AwardingOrganisationName);
    }

    [Fact]
    public async Task Index_ReturnsNotFound_WhenQueryFails()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>();
        queryResponse.Success = false;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(queryResponse);

        var qualificationQuery =
            new QualificationQuery
            {
                PageNumber = 1,
                RecordsPerPage = 10
            };

        // Act
        var result = await _controller.Index(qualificationQuery);

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

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), CancellationToken.None))
            .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.QualificationDetails("Ref123");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<ChangedQualificationDetailsViewModel>(viewResult.ViewData.Model);
    }

    [Fact(Skip = "This test is being ignored for now.")]
    [ExcludeFromCodeCoverage]
    public async Task QualificationDetails_ReturnsNotFound_WhenQueryFails()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetQualificationDetailsQueryResponse>>();
        queryResponse.Success = false;
        queryResponse.ErrorMessage = "No details found for qualification reference: Ref123";

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationDetailsQuery>(), CancellationToken.None))
            .ReturnsAsync(queryResponse);

        // Act
        try
        {
            await _controller.QualificationDetails("Ref123");
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
        Assert.IsType<RedirectResult>(result);
    }

    [Fact]
    public async Task Clear_Empty()
    {
        // Arrange
        var queryResponse = _fixture.Create<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>();
        queryResponse.Success = true;
        queryResponse.Value.Data = _fixture.CreateMany<ChangedQualification>(2).ToList();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), CancellationToken.None))
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

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetChangedQualificationsQuery>(), CancellationToken.None))
                     .ReturnsAsync(queryResponse);

        QualificationQuery qualificationQuery =
            new QualificationQuery
            {
                PageNumber = 1,
                RecordsPerPage = 10,

            };

        // Act
        var result = await _controller.ChangePage(qualificationQuery, newPage: 2);

        // Assert
        var viewResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", viewResult.ActionName);
    }

    [Fact]
    public async Task Search()
    {
        // Arrange
        var viewModel = new ChangedQualificationsViewModel
        {
            PaginationViewModel = new PaginationViewModel(10, 1, 1),
            Filter = new NewQualificationFilterViewModel()
        };

        // Act
        var result = await _controller.Search(viewModel);

        // Assert
        var viewResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", viewResult.ActionName);
        Assert.Equal(viewModel.PaginationViewModel.RecordsPerPage, viewResult.RouteValues!["recordsPerPage"]);
        Assert.Equal(viewModel.Filter.Organisation, viewResult.RouteValues["organisation"]);
        Assert.Equal(viewModel.Filter.QualificationName, viewResult.RouteValues["name"]);
        Assert.Equal(viewModel.Filter.QAN, viewResult.RouteValues["qan"]);
    }
}