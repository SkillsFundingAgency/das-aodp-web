using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.Qualifications;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class QualificationSearchControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<QualificationSearchController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly QualificationSearchController _controller;

    public QualificationSearchControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<QualificationSearchController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _controller = new QualificationSearchController(_loggerMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Index_WithSearchCriteria_CallsGetQualificationsAndMapsResults()
    {
        // Arrange
        var model = _fixture.Build<QualificationSearchViewModel>()
            .With(m => m.SearchTerm, "search-term")
            .With(m => m.Pagination, new PaginationViewModel { RecordsPerPage = 10, CurrentPage = 1 })
            .Create();

        // create a qualifications response with items
        var qualificationBody = _fixture.Build<GetQualificationsQueryResponse>()
            .With(r => r.Qualifications, new List<GetMatchingQualificationsQueryItem>
            {
                        new GetMatchingQualificationsQueryItem { Qan = "QAN1", QualificationName = "Title 1", Status = Guid.NewGuid() },
                        new GetMatchingQualificationsQueryItem { Qan = "QAN2", QualificationName = "Title 2", Status = Guid.NewGuid() }
            })
            .With(r => r.TotalRecords, 2)
            .With(r => r.Skip, 0)
            .With(r => r.Take, 10)
            .Create();

        var qualificationsResponseBody = _fixture.Build<BaseMediatrResponse<GetQualificationsQueryResponse>>()
            .With(r => r.Value, qualificationBody)
            .With(r => r.Success, true)
            .Create();

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetQualificationsQuery>(q => q.SearchTerm == model.SearchTerm), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualificationsResponseBody);

        // also ensure process statuses call returns an empty list so Map can run safely
        var procStatusesBody = _fixture.Build<GetProcessStatusesQueryResponse>()
            .With(p => p.ProcessStatuses, new List<GetProcessStatusesQueryResponse.ProcessStatus>())
            .Create();

        var procStatusesResponse = _fixture.Build<BaseMediatrResponse<GetProcessStatusesQueryResponse>>()
            .With(r => r.Value, procStatusesBody)
            .With(r => r.Success, true)
            .Create();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(procStatusesResponse);

        // job runs - empty to avoid setting dates
        var jobRunsBody = _fixture.Build<GetJobRunsQueryResponse>()
            .With(w => w.JobRuns, new List<JobRun>())
            .Create();

        var emptyJobRunsResponse = _fixture.Build<BaseMediatrResponse<GetJobRunsQueryResponse>>()
            .With(r => r.Value, jobRunsBody)
            .With(r => r.Success, true)
            .Create();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetJobRunsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyJobRunsResponse);

        // Act - pageNumber > 0 so controller uses GetQualifications
        var result = await _controller.Index("search-term", 1, 10);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsAssignableFrom<QualificationSearchViewModel>(viewResult.ViewData.Model);
        Assert.NotNull(vm.Qualifications);
        Assert.Equal(2, vm.Qualifications.Count());
        Assert.Contains(vm.Qualifications, r => r.Qan == "QAN1" && r.QualificationName == "Title 1");
        Assert.Equal(1, vm.Pagination.CurrentPage);
        Assert.Equal(10, vm.Pagination.RecordsPerPage);
    }

    [Fact]
    public async Task Index_SetsRegulatedAndFundedLastImported_WhenJobRunsPresent()
    {
        // Arrange
        var regulatedRun = new JobRun
        {
            StartTime = DateTime.UtcNow.AddDays(-2),
            EndTime = DateTime.UtcNow.AddDays(-1)
        };

        var fundedRun = new JobRun
        {
            StartTime = DateTime.UtcNow.AddDays(-4),
            EndTime = DateTime.UtcNow.AddDays(-3)
        };

        var regulatedBody = _fixture.Build<GetJobRunsQueryResponse>()
            .With(r => r.JobRuns, new List<JobRun> { regulatedRun })
            .Create();

        var regulatedResponse = _fixture.Build<BaseMediatrResponse<GetJobRunsQueryResponse>>()
            .With(r => r.Value, regulatedBody)
            .With(r => r.Success, true)
            .Create();

        var fundedBody = _fixture.Build<GetJobRunsQueryResponse>()
            .With(r => r.JobRuns, new List<JobRun> { fundedRun })
            .Create();

        var fundedResponse = _fixture.Build<BaseMediatrResponse<GetJobRunsQueryResponse>>()
            .With(r => r.Value, fundedBody)
            .With(r => r.Success, true)
            .Create();

        // GetProcessStatusesQuery - return empty list to allow flow
        var procStatusesBody = _fixture.Build<GetProcessStatusesQueryResponse>()
            .With(p => p.ProcessStatuses, new List<GetProcessStatusesQueryResponse.ProcessStatus>())
            .Create();

        var procStatusesResponse = _fixture.Build<BaseMediatrResponse<GetProcessStatusesQueryResponse>>()
            .With(r => r.Value, procStatusesBody)
            .With(r => r.Success, true)
            .Create();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(procStatusesResponse);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetJobRunsQuery>(q => q.JobName == JobNames.RegulatedQualifications.ToString()), It.IsAny<CancellationToken>()))
            .ReturnsAsync(regulatedResponse);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetJobRunsQuery>(q => q.JobName == JobNames.FundedQualifications.ToString()), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundedResponse);

        // Act
        var result = await _controller.Index(string.Empty, 0, 0);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsAssignableFrom<QualificationSearchViewModel>(viewResult.ViewData.Model);
        Assert.Equal(regulatedRun.EndTime ?? regulatedRun.StartTime, vm.RegulatedQualificationsLastImported);
        Assert.Equal(fundedRun.EndTime ?? fundedRun.StartTime, vm.FundedQualificationsLastImported);
    }

    [Fact]
    public void Search_Post_InvalidModel_ReturnsView()
    {
        // Arrange
        var model = new QualificationSearchViewModel();
        _controller.ModelState.AddModelError("SearchTerm", "Required");

        // Act
        var result = _controller.Search(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        var vm = Assert.IsAssignableFrom<QualificationSearchViewModel>(viewResult.ViewData.Model);
        Assert.Same(model, vm);
    }

    [Fact]
    public void Search_Post_Valid_RedirectsToIndex()
    {
        // Arrange
        var model = new QualificationSearchViewModel
        {
            SearchTerm = "term",
            Pagination = new PaginationViewModel { RecordsPerPage = 25 }
        };

        // Act
        var result = _controller.Search(model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QualificationSearchController.Index), redirect.ActionName);
        Assert.Equal("term", redirect.RouteValues["searchTerm"]);
        Assert.Equal(1, redirect.RouteValues["pageNumber"]);
        Assert.Equal(25, redirect.RouteValues["recordsPerPage"]);
    }

    [Fact]
    public void ChangePage_ModelStateInvalid_ReturnsIndexView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Any", "error");

        // Act
        var result = _controller.ChangePage(2, 10, "search");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
    }

    [Fact]
    public void ChangePage_ModelStateValid_RedirectsToIndex()
    {
        // Act
        var result = _controller.ChangePage(3, 15, "s");

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QualificationSearchController.Index), redirect.ActionName);
        Assert.Equal(3, redirect.RouteValues["pageNumber"]);
        Assert.Equal(15, redirect.RouteValues["recordsPerPage"]);
        Assert.Equal("s", redirect.RouteValues["searchTerm"]);
    }

    [Fact]
    public async Task Index_WhenJobRunsMediatorThrows_ExceptionIsCaughtAndViewReturned()
    {
        // Arrange - ensure process statuses and qualifications work so exception happens only in job runs
        var procStatusesBody = _fixture.Build<GetProcessStatusesQueryResponse>()
            .With(p => p.ProcessStatuses, new List<GetProcessStatusesQueryResponse.ProcessStatus>())
            .Create();

        var procStatusesResponse = _fixture.Build<BaseMediatrResponse<GetProcessStatusesQueryResponse>>()
            .With(r => r.Value, procStatusesBody)
            .With(r => r.Success, true)
            .Create();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetProcessStatusesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(procStatusesResponse);

        var qualificationBody = _fixture.Build<GetQualificationsQueryResponse>()
            .With(r => r.Qualifications, new List<GetMatchingQualificationsQueryItem>())
            .With(r => r.TotalRecords, 0)
            .With(r => r.Skip, 0)
            .With(r => r.Take, 10)
            .Create();

        var qualificationsResponse = _fixture.Build<BaseMediatrResponse<GetQualificationsQueryResponse>>()
            .With(r => r.Value, qualificationBody)
            .With(r => r.Success, true)
            .Create();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQualificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualificationsResponse);

        // make job runs call throw for first attempt -> controller should catch and continue
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetJobRunsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        // Act
        var result = await _controller.Index("search", 1, 10);

        // Assert - no exception escapes, view returned
        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsAssignableFrom<QualificationSearchViewModel>(viewResult.ViewData.Model);
        Assert.NotNull(vm.Pagination);
        Assert.Null(vm.RegulatedQualificationsLastImported);
        Assert.Null(vm.FundedQualificationsLastImported);
    }
}
