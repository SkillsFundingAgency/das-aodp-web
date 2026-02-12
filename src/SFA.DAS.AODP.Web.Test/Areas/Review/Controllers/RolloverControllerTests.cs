using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Enums;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers;

public class RolloverControllerTests
{
    private readonly Mock<ILogger<RolloverController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly RolloverController _controller;

    public RolloverControllerTests()
    {
        _loggerMock = new Mock<ILogger<RolloverController>>();
        _mediatorMock = new Mock<IMediator>();
        _controller = new RolloverController(_loggerMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public void Index_Get_ReturnsViewWithModel()
    {
        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("RolloverStart", viewResult.ViewName);
        Assert.IsType<RolloverStartViewModel>(viewResult.Model);
    }

    [Fact]
    public void Index_Post_InvalidModelState_ReturnsStartViewWithModel()
    {
        // Arrange
        _controller.ModelState.AddModelError("SelectedProcess", "required");
        var vm = new RolloverStartViewModel();

        // Act
        var result = _controller.Index(vm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("RolloverStart", viewResult.ViewName);
        Assert.Same(vm, viewResult.Model);
    }

    [Fact]
    public void Index_Post_SelectedProcessInitialSelection_RedirectsToInitialSelection()
    {
        // Arrange
        var vm = new RolloverStartViewModel
        {
            SelectedProcess = RolloverProcess.InitialSelection
        };

        // Act
        var result = _controller.Index(vm);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.InitialSelection), redirect.ActionName);
    }

    [Fact]
    public void Index_Post_SelectedProcessFinalUpload_RedirectsToUploadQualifications()
    {
        // Arrange
        var vm = new RolloverStartViewModel
        {
            SelectedProcess = RolloverProcess.FinalUpload
        };

        // Act
        var result = _controller.Index(vm);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.UploadQualifications), redirect.ActionName);
    }

    [Fact]
    public void InitialSelection_Get_ReturnsViewAndSetsTitle()
    {
        // Act
        var result = _controller.InitialSelection();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Initial selection of qualificaton", viewResult.ViewData["Title"]);
    }

    [Fact]
    public void UploadQualifications_Get_ReturnsViewAndSetsTitle()
    {
        // Act
        var result = _controller.UploadQualifications();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Upload qualifications to RollOver", viewResult.ViewData["Title"]);
    }

    [Fact]
    public async Task CheckData_Get_PopulatesModelWithLatestJobRunDates_AndReturnsView()
    {
        // Arrange
        var regulatedDate = new DateTime(2025, 11, 25, 10, 0, 0);
        var fundedDate = new DateTime(2025, 11, 26, 11, 0, 0);
        var defundingDate = new DateTime(2025, 11, 27, 12, 0, 0);
        var pldnsDate = new DateTime(2025, 11, 27, 13, 0, 0);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<IRequest<BaseMediatrResponse<GetJobRunsQueryResponse>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IRequest<BaseMediatrResponse<GetJobRunsQueryResponse>> req, CancellationToken ct) =>
            {
                var query = req as GetJobRunsQuery;
                var jobName = query?.JobName ?? string.Empty;

                var response = new BaseMediatrResponse<GetJobRunsQueryResponse>
                {
                    Success = true,
                    Value = new GetJobRunsQueryResponse
                    {
                        JobRuns = new List<JobRun>()
                    }
                };

                var run = new JobRun
                {
                    Id = Guid.NewGuid(),
                    Status = "Completed",
                    StartTime = DateTime.MinValue,
                    EndTime = DateTime.MinValue,
                    User = "tester",
                    JobId = Guid.NewGuid()
                };

                if (string.Equals(jobName, JobNames.RegulatedQualifications.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    run.EndTime = regulatedDate;
                }
                else if (string.Equals(jobName, JobNames.FundedQualifications.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    run.EndTime = fundedDate;
                }
                else if (string.Equals(jobName, JobNames.DefundingList.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    run.EndTime = defundingDate;
                }
                else if (string.Equals(jobName, JobNames.Pldns.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    run.EndTime = pldnsDate;
                }

                response.Value.JobRuns.Add(run);
                return response;
            });

        // Act
        var result = await _controller.CheckData();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CheckData", viewResult.ViewName);

        var model = Assert.IsType<RolloverImportStatusViewModel>(viewResult.Model);
        Assert.Equal(regulatedDate, model.RegulatedQualificationsLastImported);
        Assert.Equal(fundedDate, model.FundedQualificationsLastImported);
        Assert.Equal(defundingDate, model.DefundingListLastImported);
        Assert.Equal(pldnsDate, model.PldnsListLastImported);
    }

    [Fact]
    public void CheckData_Post_InvalidModelState_ReturnsViewWithModel()
    {
        // Arrange
        _controller.ModelState.AddModelError("dummy", "error");
        var vm = new RolloverImportStatusViewModel();

        // Act
        var result = _controller.CheckData(vm);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("CheckData", viewResult.ViewName);
        Assert.Same(vm, viewResult.Model);
    }

    [Fact]
    public void CheckData_Post_ValidModel_RedirectsToInitialSelection()
    {
        // Arrange
        var vm = new RolloverImportStatusViewModel();

        // Act
        var result = _controller.CheckData(vm);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RolloverController.InitialSelection), redirect.ActionName);
    }
}