using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class OutputFileControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<OutputFileController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly OutputFileController _controller;

    public OutputFileControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<OutputFileController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
       _controller = new OutputFileController(_loggerMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult()
    {
        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName); // default view
    }

    [Fact]
    public async Task GetOutputFile_ReturnsFile_WhenQuerySucceeds()
    {
        // Arrange
        var bytes = new byte[] { 1, 2, 3 };
        var payload = _fixture.Build<GetQualificationOutputFileResponse>()
                              .With(p => p.FileName, "qualifications_export.zip")
                              .With(p => p.ZipFileContent, bytes)
                              .With(p => p.ContentType, "application/zip")
                              .Create();

        var mediatorResponse = _fixture.Build<BaseMediatrResponse<GetQualificationOutputFileResponse>>()
                                       .With(r => r.Success, true)
                                       .With(r => r.Value, payload)
                                       .Create();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(mediatorResponse);

        // Act
        var result = await _controller.GetOutputFile(CancellationToken.None);

        // Assert
        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/zip", file.ContentType);
        Assert.Equal("qualifications_export.zip", file.FileDownloadName);
        Assert.Equal(bytes, file.FileContents);
    }

    [Fact]
    public async Task GetOutputFile_ReturnsProblem_WhenQueryFails()
    {
        // Arrange: failure (no value / empty content or Success=false)
        var mediatorResponse = _fixture.Build<BaseMediatrResponse<GetQualificationOutputFileResponse>>()
                                       .With(r => r.Success, false)
                                       .With(r => r.ErrorMessage, "Output file not available.")
                                       .With(r => r.Value, (GetQualificationOutputFileResponse?)null)
                                       .Create();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(mediatorResponse);

        // Act
        var result = await _controller.GetOutputFile(CancellationToken.None);

        // Assert
        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, obj.StatusCode); 
        var problem = Assert.IsType<ProblemDetails>(obj.Value);
        Assert.Equal("Output file not available.", problem.Detail);
    }
}
