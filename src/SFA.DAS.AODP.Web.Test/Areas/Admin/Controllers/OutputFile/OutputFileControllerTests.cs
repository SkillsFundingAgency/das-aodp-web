using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.OutputFile;
using SFA.DAS.AODP.Application.Queries.OutputFile;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers;
using SFA.DAS.AODP.Web.Models.OutputFile;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Admin.Controllers.OutputFile;

public class OutputFileControllerTests
{
    private readonly OutputFileController _controller;
    private readonly Mock<IMediator> mediator = new();
    private readonly Mock<ILogger<OutputFileController>> logger = new();
    private readonly Fixture _fixture = new();

    public OutputFileControllerTests()
    {
        _controller = new(mediator.Object, logger.Object);
    }

    public async Task Get_IndexPage_ReturnsOk_WhenSucessful()
    {
        var expectedResult = _fixture.Create<BaseMediatrResponse<GetPreviousOutputFilesQueryResponse>>();
        expectedResult.Success = true;
        mediator.Setup(v => v.Send(It.IsAny<GetPreviousOutputFilesQuery>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(expectedResult));

        var response = _controller.Index();
        var viewResult = Assert.IsType<ViewResult>(response);
        var resultViewModel = Assert.IsAssignableFrom<GenerateViewModel>(viewResult.ViewData.Model);
    }

    public async Task Get_IndexPage_ReturnsRedirect_WhenError()
    {
        var expectedException = _fixture.Create<Exception>();
        mediator.Setup(v => v.Send(It.IsAny<GetPreviousOutputFilesQuery>(), It.IsAny<CancellationToken>()))
            .Throws(expectedException);

        var response = _controller.Index();
        var redirect = Assert.IsType<RedirectResult>(response);
        Assert.EndsWith("Home/Error", redirect.Url);
    }

    public async Task Post_IndexPage_ReturnsRedirect_WhenSucessful()
    {
        var expectedResult = _fixture.Create<BaseMediatrResponse<GetPreviousOutputFilesQueryResponse>>();
        expectedResult.Success = true;
        mediator.Setup(v => v.Send(It.IsAny<GetPreviousOutputFilesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var response = _controller.Index(new GenerateViewModel()
        {
            AdditionalFormActions = new GenerateViewModel.FormActions
            {
                GenerateFile = false
            }
        });
        var redirect = Assert.IsType<RedirectToActionResult>(response);
        Assert.Equal("OutputFile", redirect.ControllerName);
        Assert.Equal("Index", redirect.ActionName);
    }

    public async Task Post_IndexPage_GenerateNewOutputFile_ReturnsRedirect_WhenSucessful()
    {
        var expectedResult = _fixture.Create<BaseMediatrResponse<EmptyResponse>>();
        expectedResult.Success = true;
        mediator.Setup(v => v.Send(It.IsAny<GenerateNewOutputFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var response = _controller.Index(new GenerateViewModel()
        {
            AdditionalFormActions = new GenerateViewModel.FormActions
            {
                GenerateFile = true
            }
        });
        var redirect = Assert.IsType<RedirectToActionResult>(response);
        Assert.Equal("OutputFile", redirect.ControllerName);
        Assert.Equal("Index", redirect.ActionName);
    }

    public async Task Get_Download_ReturnsFileResult_WhenSucessful()
    {
        var expectedResult = _fixture.Create<BaseMediatrResponse<GetOutputFileDownloadQueryResponse>>();
        expectedResult.Success = true;
        mediator.Setup(v => v.Send(It.IsAny<GetOutputFileDownloadQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var response = _controller.Download("test2.txt");
        var fileStreamResult = Assert.IsType<FileStreamResult>(response);
        Assert.Equal("test2.txt", fileStreamResult.FileDownloadName);
        Assert.Equal(expectedResult.Value.ContentType, fileStreamResult.ContentType);
    }

    public async Task Get_Download_ReturnsRedirect_OnError()
    {
        var expectedException = _fixture.Create<Exception>();
        mediator.Setup(v => v.Send(It.IsAny<GetOutputFileDownloadQuery>(), It.IsAny<CancellationToken>()))
            .Throws(expectedException);

        var response = _controller.Download("test2.txt");
        var fileStreamResult = Assert.IsType<FileStreamResult>(response);
        var redirect = Assert.IsType<RedirectResult>(response);
        Assert.EndsWith("Home/Error", redirect.Url);
    }
}
