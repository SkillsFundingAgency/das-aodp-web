using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Import;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Import;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class ImportControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<ImportController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IUserHelperService> _userHelpService;
    private readonly ImportController _controller;

    public ImportControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<ImportController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _userHelpService = _fixture.Freeze<Mock<IUserHelperService>>();
        _controller = new ImportController(_loggerMock.Object, _mediatorMock.Object, _userHelpService.Object);
    }

    [Fact]
    public void Index_ReturnsViewResult()
    {
        // Arrange 

        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ImportRequestViewModel>(viewResult.ViewData.Model);
    }

    [Fact]
    public void SelectImport_ReturnsViewResult()
    {
        // Arrange
        var viewModel = _fixture.Create<ImportRequestViewModel>();

        // Act
        var result = _controller.SelectImport(viewModel);

        // Assert
        var viewResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ConfirmImportSelection", viewResult.ActionName);        
    }

    [Fact]
    public void ConfirmImportSelection_ReturnsViewResult()
    {
        // Arrange
        var viewModel = _fixture.Create<ImportRequestViewModel>();

        // Act
        var result = _controller.ConfirmImportSelection(viewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ConfirmImportRequestViewModel>(viewResult.ViewData.Model);
    }

    [Fact]
    public async Task ConfirmImportSelection_ReturnsViewResult_WithRequestedStatus()
    {
        // Arrange
        var viewModel = _fixture.Build<ConfirmImportRequestViewModel>()
                            .With(w => w.ImportType, "Regulated Qualifications")
                            .Create();
        var queryResponse = _fixture.Create<BaseMediatrResponse<EmptyResponse>>();
        queryResponse.Success = true;
        var userName = "TestUser";
        _userHelpService.Setup(s => s.GetUserDisplayName()).Returns(userName);
        _mediatorMock.Setup(m => m.Send(It.Is<RequestJobRunCommand>(i => i.JobName == JobNames.RegulatedQualifications.ToString()
                                                                            && i.UserName == userName), default))
                     .ReturnsAsync(queryResponse);

        var body = _fixture.Build<GetJobRunsQueryResponse>()
            .With(w => w.JobRuns, new List<JobRun>())
            .Create();

        var currentJobRuns = _fixture.Build<BaseMediatrResponse<GetJobRunsQueryResponse>>()
            .With(w => w.Value, body)
            .With(w => w.Success, true)
            .Create();
     
        _mediatorMock.Setup(m => m.Send(It.Is<GetJobRunsQuery>(i => i.JobName == JobNames.RegulatedQualifications.ToString()), default))
                     .ReturnsAsync(currentJobRuns);

        // Act
        var result = await _controller.ConfirmImportSelection(viewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("SubmitImportRequest", viewResult.ViewName);
        var model = Assert.IsAssignableFrom<SubmitImportRequestViewModel>(viewResult.ViewData.Model);              
        Assert.Equal(userName, model.UserName);
        Assert.Equal(viewModel.ImportType, model.ImportType);
        Assert.Equal(JobStatus.Requested.ToString(), model.Status);
    }

    [Fact]
    public async Task ConfirmImportSelection_ReturnsViewResult_JobsAlreadyRunning()
    {
        // Arrange
        var viewModel = _fixture.Build<ConfirmImportRequestViewModel>()
                            .With(w => w.ImportType, "Regulated Qualifications")
                            .Create();

        var userName = "TestUser";
        _userHelpService.Setup(s => s.GetUserDisplayName()).Returns(userName);


        var currentRun = _fixture.Build<JobRun>()
                                    .With(w => w.Status, JobStatus.Running.ToString())
                                    .With(w => w.StartTime, DateTime.Now)
                                    .Create();

        var body = _fixture.Build<GetJobRunsQueryResponse>()
            .With(w => w.JobRuns, new List<JobRun>() { currentRun })
            .Create();

        var currentJobRuns = _fixture.Build<BaseMediatrResponse<GetJobRunsQueryResponse>>()
            .With(w => w.Value, body)
            .Create();
        
        _mediatorMock.Setup(m => m.Send(It.Is<GetJobRunsQuery>(i => i.JobName == JobNames.RegulatedQualifications.ToString()), default))
                     .ReturnsAsync(currentJobRuns);

        // Act
        var result = await _controller.ConfirmImportSelection(viewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("SubmitImportRequest", viewResult.ViewName);
        var model = Assert.IsAssignableFrom<SubmitImportRequestViewModel>(viewResult.ViewData.Model);
        Assert.Equal(currentRun.User, model.UserName);
        Assert.Equal(viewModel.ImportType, model.ImportType);
        Assert.Equal(JobStatus.Requested.ToString(), model.Status);
    }

    [Fact]
    public void SubmitImportRequest_ReturnsViewResult()
    {
        // Arrange
        var viewModel = _fixture.Create<SubmitImportRequestViewModel>();

        // Act
        var result = _controller.SubmitImportRequest(viewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<SubmitImportRequestViewModel>(viewResult.ViewData.Model);
    }

    [Fact]
    public async Task CheckProgress_ReturnsViewResult_WithInitialJobLookup()
    {
        // Arrange
        var viewModel = _fixture.Build<SubmitImportRequestViewModel>()
                            .With(w => w.ImportType, "Regulated Qualifications")
                            .With(w => w.JobName, JobNames.RegulatedQualifications.ToString())
                            .With(w => w.JobRunId, Guid.Empty)
                            .Create();
        var oldRun = _fixture.Build<JobRun>()
                                    .With(w => w.Status, JobStatus.Completed.ToString())
                                    .With(w => w.StartTime, DateTime.Now.AddDays(-1))
                                    .Create();
        var currentRun = _fixture.Build<JobRun>()
                                    .With(w => w.Status, JobStatus.Running.ToString())
                                    .With(w => w.StartTime, DateTime.Now)
                                    .Create();

        var body = _fixture.Build<GetJobRunsQueryResponse>()
            .With(w => w.JobRuns, new List<JobRun>() { oldRun, currentRun })
            .Create();

        var queryResponse = _fixture.Build<BaseMediatrResponse<GetJobRunsQueryResponse>>()
            .With(w => w.Value, body)
            .Create();

        queryResponse.Success = true;

        var userName = "TestUser";
        _userHelpService.Setup(s => s.GetUserDisplayName()).Returns(userName);

        _mediatorMock.Setup(m => m.Send(It.Is<GetJobRunsQuery>(i => i.JobName == viewModel.JobName), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.CheckProgress(viewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("SubmitImportRequest", viewResult.ViewName);
        var model = Assert.IsAssignableFrom<SubmitImportRequestViewModel>(viewResult.ViewData.Model);
        Assert.Equal(currentRun.User, model.UserName);
        Assert.Equal(currentRun.Status, model.Status);
        Assert.Equal(currentRun.StartTime, model.SubmittedTime);
    }

    [Fact]
    public async Task CheckProgress_ReturnsViewResult_SubsequentLookup()
    {
        // Arrange
        var viewModel = _fixture.Build<SubmitImportRequestViewModel>()
                            .With(w => w.ImportType, "Regulated Qualifications")
                            .With(w => w.JobName, JobNames.RegulatedQualifications.ToString())
                            .With(w => w.JobRunId, Guid.NewGuid())
                            .Create();
       
        var currentRun = _fixture.Build<JobRun>()
                                    .With(w => w.Status, JobStatus.Running.ToString())
                                    .With(w => w.StartTime, DateTime.Now)
                                    .Create();      

        var queryResponse = _fixture.Build<BaseMediatrResponse<GetJobRunByIdQueryResponse>>()
            .With(w => w.Value, currentRun)
            .Create();

        queryResponse.Success = true;

        var userName = "TestUser";
        _userHelpService.Setup(s => s.GetUserDisplayName()).Returns(userName);

        _mediatorMock.Setup(m => m.Send(It.Is<GetJobRunByIdQuery>(i => i.Id == viewModel.JobRunId), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.CheckProgress(viewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("SubmitImportRequest", viewResult.ViewName);
        var model = Assert.IsAssignableFrom<SubmitImportRequestViewModel>(viewResult.ViewData.Model);
        Assert.Equal(currentRun.User, model.UserName);
        Assert.Equal(currentRun.Status, model.Status);
        Assert.Equal(currentRun.StartTime, model.SubmittedTime);
    }

    [Fact]
    public async Task CheckProgress_ReturnsViewResult_Completed()
    {
        // Arrange
        var viewModel = _fixture.Build<SubmitImportRequestViewModel>()
                            .With(w => w.ImportType, "Regulated Qualifications")
                            .With(w => w.JobName, JobNames.RegulatedQualifications.ToString())
                            .With(w => w.JobRunId, Guid.NewGuid())
                            .Create();

        var currentRun = _fixture.Build<JobRun>()
                                    .With(w => w.Status, JobStatus.Completed.ToString())
                                    .With(w => w.StartTime, DateTime.Now)
                                    .Create();

        var queryResponse = _fixture.Build<BaseMediatrResponse<GetJobRunByIdQueryResponse>>()
            .With(w => w.Value, currentRun)
            .Create();

        queryResponse.Success = true;

        var userName = "TestUser";
        _userHelpService.Setup(s => s.GetUserDisplayName()).Returns(userName);

        _mediatorMock.Setup(m => m.Send(It.Is<GetJobRunByIdQuery>(i => i.Id == viewModel.JobRunId), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.CheckProgress(viewModel);

        // Assert
        var viewResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Complete", viewResult.ActionName);        
    }

    [Fact]
    public async Task Complete_ReturnsViewResult()
    {
        // Arrange
        var viewModel = _fixture.Build<CompleteViewModel>()
                            .With(w => w.ImportType, "Regulated Qualifications")
                            .With(w => w.JobName, JobNames.RegulatedQualifications.ToString())
                            .With(w => w.JobRunId, Guid.NewGuid())
                            .Create();

        var currentRun = _fixture.Build<JobRun>()
                                    .With(w => w.Status, JobStatus.Completed.ToString())
                                    .With(w => w.StartTime, DateTime.Now)
                                    .Create();

        var queryResponse = _fixture.Build<BaseMediatrResponse<GetJobRunByIdQueryResponse>>()
            .With(w => w.Value, currentRun)
            .Create();

        queryResponse.Success = true;

        var userName = "TestUser";
        _userHelpService.Setup(s => s.GetUserDisplayName()).Returns(userName);

        _mediatorMock.Setup(m => m.Send(It.Is<GetJobRunByIdQuery>(i => i.Id == viewModel.JobRunId), default))
                     .ReturnsAsync(queryResponse);

        // Act
        var result = await _controller.Complete(viewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);       
        var model = Assert.IsAssignableFrom<CompleteViewModel>(viewResult.ViewData.Model);
        Assert.Equal(currentRun.User, model.UserName);
        Assert.Equal(currentRun.Status, model.Status);
        Assert.Equal(currentRun.EndTime, model.CompletedTime);
    }
}
