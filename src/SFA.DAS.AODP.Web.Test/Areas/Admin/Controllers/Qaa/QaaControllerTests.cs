using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.QaaDownload;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers;
using SFA.DAS.AODP.Web.Models.Qaa;
using System.Security.Claims;

namespace SFA.DAS.AODP.Web.Test.Controllers;

public class QaaControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<QaaController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly QaaController _controller;

    public QaaControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _loggerMock = _fixture.Freeze<Mock<ILogger<QaaController>>>();
        _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
        _controller = new QaaController(_loggerMock.Object, _mediatorMock.Object);
    }

    private void SetUser(string? name)
    {
        var identity = name is null
            ? new ClaimsIdentity()
            : new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, name) }, "TestAuth");

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    [Fact]
    public async Task Index_ReturnsView_WithMappedViewModel()
    {
        // Arrange
        var lastImported = new DateTime(2026, 2, 17);
        var mostRecentDownload = new DateTime(2026, 3, 5);

        var response = new BaseMediatrResponse<GetQaaDownloadSummaryQueryResponse>
        {
            Success = true,
            Value = new GetQaaDownloadSummaryQueryResponse
            {
                DataLastImportedDate = lastImported,
                MostRecentDownloadDate = mostRecentDownload,
                NewQualificationsCount = 1,
                ExtendedQualificationsCount = 2,
                DiscontinuedQualificationsCount = 3,
                DownloadHistory = new List<GetQaaDownloadSummaryQueryResponse.QaaDownloadLog>
                {
                    new() { UserDisplayName = "tester", DownloadDate = mostRecentDownload }
                }
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetQaaDownloadSummaryQuery>(), default))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QaaViewModel>(viewResult.Model);

        Assert.Equal(lastImported, model.DataLastImportedDate);
        Assert.Equal(mostRecentDownload, model.MostRecentDownloadDate);
        Assert.Equal(1, model.NewQualificationsCount);
        Assert.Equal(2, model.ExtendedQualificationsCount);
        Assert.Equal(3, model.DiscontinuedQualificationsCount);
        var historyEntry = Assert.Single(model.DownloadHistory);
        Assert.Equal("tester", historyEntry.UserDisplayName);
        Assert.Equal(mostRecentDownload, historyEntry.DownloadDate);
    }

    [Fact]
    public async Task Download_ReturnsFileResult_WithApiResponseContent_AndCurrentUsername()
    {
        // Arrange
        SetUser("tester");

        var response = new BaseMediatrResponse<GetQaaQualificationsExportQueryResponse>
        {
            Success = true,
            Value = new GetQaaQualificationsExportQueryResponse
            {
                FileContent = new byte[] { 1, 2, 3 },
                FileName = "export.csv",
                ContentType = "text/csv"
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetQaaQualificationsExportQuery>(q => q.CurrentUsername == "tester"), default))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Download();

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal(new byte[] { 1, 2, 3 }, fileResult.FileContents);
        Assert.Equal("text/csv", fileResult.ContentType);
        Assert.Equal("export.csv", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task Download_WhenUserIdentityNameIsNull_UsesEmptyUsername()
    {
        // Arrange
        SetUser(null);

        var response = new BaseMediatrResponse<GetQaaQualificationsExportQueryResponse>
        {
            Success = true,
            Value = new GetQaaQualificationsExportQueryResponse
            {
                FileContent = new byte[] { 1 },
                FileName = "export.csv",
                ContentType = "text/csv"
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetQaaQualificationsExportQuery>(q => q.CurrentUsername == string.Empty), default))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Download();

        // Assert
        Assert.IsType<FileContentResult>(result);
        _mediatorMock.Verify(
            m => m.Send(It.Is<GetQaaQualificationsExportQuery>(q => q.CurrentUsername == string.Empty), default),
            Times.Once);
    }
}
