using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application.Exceptions;
using SFA.DAS.AODP.Web.Middleware;

namespace SFA.DAS.AODP.Web.UnitTests.Middleware;

public class MediatrExceptionLoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenMediatrRequestExceptionIsThrown_ShouldLogSafeRequestMetadataAndRethrow()
    {
        // Arrange
        var logger = new Mock<ILogger<MediatrExceptionLoggingMiddleware>>();
        var exception = new InvalidOperationException("API failed");
        var mediatrException = new MediatrRequestException("SensitiveQuery", exception);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = "/Review/Rollover/CheckYourAnswers";
        var middleware = new MediatrExceptionLoggingMiddleware(_ => throw mediatrException, logger.Object);

        // Act
        var result = await Should.ThrowAsync<MediatrRequestException>(() => middleware.InvokeAsync(httpContext));

        // Assert
        result.ShouldBe(mediatrException);
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains("SensitiveQuery") &&
                    state.ToString()!.Contains("POST") &&
                    state.ToString()!.Contains("/Review/Rollover/CheckYourAnswers")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("LevelIds")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
