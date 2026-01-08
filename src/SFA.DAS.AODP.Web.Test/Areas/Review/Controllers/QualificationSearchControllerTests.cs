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
    public async Task Index_NoSearchCriteria_ReturnsViewWithEmptyResults()
    {
        // Arrange
        var model = new QualificationSearchViewModel();

        var body = _fixture.Build<GetJobRunsQueryResponse>()
            .With(w => w.JobRuns, new List<JobRun>())
            .Create();

        var emptyJobRunsResponse = _fixture.Build <BaseMediatrResponse<GetJobRunsQueryResponse>>()
            .With(r => r.Value, body)
            .With(r => r.Success, true)
            .Create();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetJobRunsQuery>(), default))
            .ReturnsAsync(emptyJobRunsResponse);
        // Act
        var result = await _controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", viewResult.ViewName);
        var vm = Assert.IsAssignableFrom<QualificationSearchViewModel>(viewResult.ViewData.Model);
        Assert.NotNull(vm.Pagination);
        Assert.True(vm.Results == null || !vm.Results.Any());
        Assert.Null(vm.RegulatedQualificationsLastImported);
        Assert.Null(vm.FundedQualificationsLastImported);
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
            .With(r => r.Data, new List<QualificationSearchResult>
            {
                    new QualificationSearchResult { Reference = "QAN1", Title = "Title 1", Status = "Live" },
                    new QualificationSearchResult { Reference = "QAN2", Title = "Title 2", Status = "Archived" }
            })
            .With(r => r.TotalRecords, 2)
            .Create();

        var qualificationsResponseBody = _fixture.Build<BaseMediatrResponse<GetQualificationsQueryResponse>>()
            .With(r => r.Value, qualificationBody)
            .With(r => r.Success, true)
            .Create();

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetQualificationsQuery>(q => q.Name == model.SearchTerm), It.IsAny<CancellationToken>()))
            .ReturnsAsync(qualificationsResponseBody);

        // job runs - empty to avoid setting dates
        var body = _fixture.Build<GetJobRunsQueryResponse>()
            .With(w => w.JobRuns, new List<JobRun>())
            .Create();

        var emptyJobRunsResponse = _fixture.Build<BaseMediatrResponse<GetJobRunsQueryResponse>>()
            .With(r => r.Value, body)
            .With(r => r.Success, true)
            .Create();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetJobRunsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyJobRunsResponse);

        // Act
        var result = await _controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsAssignableFrom<QualificationSearchViewModel>(viewResult.ViewData.Model);
        Assert.NotNull(vm.Results);
        Assert.Equal(2, vm.Results.Count());
        Assert.Contains(vm.Results, r => r.Reference == "QAN1" && r.Title == "Title 1");
    }

    [Fact]
    public async Task Index_SetsRegulatedAndFundedLastImported_WhenJobRunsPresent()
    {
        // Arrange
        var model = new QualificationSearchViewModel(); // no search criteria

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

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetJobRunsQuery>(q => q.JobName == JobNames.RegulatedQualifications.ToString()), It.IsAny<CancellationToken>()))
            .ReturnsAsync(regulatedResponse);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetJobRunsQuery>(q => q.JobName == JobNames.FundedQualifications.ToString()), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fundedResponse);

        // Act
        var result = await _controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsAssignableFrom<QualificationSearchViewModel>(viewResult.ViewData.Model);
        Assert.Equal(regulatedRun.EndTime ?? regulatedRun.StartTime, vm.RegulatedQualificationsLastImported);
        Assert.Equal(fundedRun.EndTime ?? fundedRun.StartTime, vm.FundedQualificationsLastImported);
    }
}
