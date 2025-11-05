using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers;
using SFA.DAS.AODP.Web.Enums;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class OutputFileControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<OutputFileController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly OutputFileController _controller;

    private const string OutputFileErrorMessage = "File error message.";

    public OutputFileControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<OutputFileController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
       _controller = new OutputFileController(_loggerMock.Object, _mediatorMock.Object);

        var http = new DefaultHttpContext();
        var tempData = new TempDataDictionary(http, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
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
    public async Task Index_ReturnsViewResult_And_UsesTempDataMessageWhenPresent()
    {
        // Arrange
        _controller.TempData[OutputFileController.OutputFileFailed] = true;
        _controller.TempData[$"{OutputFileController.OutputFileFailed}:Message"] = OutputFileErrorMessage;

        // Act
        var result = await _controller.Index();

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Multiple(() =>
        {
            Assert.Null(view.ViewName);
            Assert.Equal(ViewNotificationMessageType.Error, view.ViewData["NotificationType"]);
            Assert.Equal(OutputFileErrorMessage, view.ViewData["NotificationMessage"]);
        });
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
    public async Task GetOutputFile_Failure_SetsTempData_AndRedirectsToIndex()
    {
        // Arrange
        var mediatorResponse = _fixture.Build<BaseMediatrResponse<GetQualificationOutputFileResponse>>()
                                       .With(r => r.Success, false)
                                       .With(r => r.ErrorMessage, OutputFileErrorMessage)
                                       .With(r => r.Value, (GetQualificationOutputFileResponse?)null)
                                       .Create();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(mediatorResponse);

        // Act
        var result = await _controller.GetOutputFile(CancellationToken.None);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(OutputFileController.Index), redirect.ActionName);
            Assert.True((bool)_controller.TempData[OutputFileController.OutputFileFailed]);
            Assert.Equal(OutputFileErrorMessage,
                _controller.TempData[$"{OutputFileController.OutputFileFailed}:Message"]);
        });
    }

    [Fact]
    public async Task GetOutputFile_Failure_WithNullMessage_UsesDefaultText()
    {
        // Arrange
        var mediatorResponse = _fixture.Build<BaseMediatrResponse<GetQualificationOutputFileResponse>>()
                                       .With(r => r.Success, false)
                                       .With(r => r.ErrorMessage, (string?)null)
                                       .With(r => r.Value, (GetQualificationOutputFileResponse?)null)
                                       .Create();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(mediatorResponse);

        // Act
        var result = await _controller.GetOutputFile(CancellationToken.None);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Multiple(() =>
        {
            Assert.Equal(nameof(OutputFileController.Index), redirect.ActionName);
            Assert.True((bool)_controller.TempData[OutputFileController.OutputFileFailed]);
            Assert.Equal(OutputFileController.OutputFileDefaultErrorMessage,
                _controller.TempData[$"{OutputFileController.OutputFileFailed}:Message"]);
        });
    }
}
