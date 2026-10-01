using Moq;
using SFA.DAS.AODP.Application.Queries.QaaDownload;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.QaaDownload;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.QaaDownload
{
    public class GetQaaDownloadSummaryQueryHandlerTests
    {
        private readonly Mock<IApiClient> _mockApiClient;
        private readonly GetQaaDownloadSummaryQueryHandler _handler;

        public GetQaaDownloadSummaryQueryHandlerTests()
        {
            _mockApiClient = new Mock<IApiClient>();
            _handler = new GetQaaDownloadSummaryQueryHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenApiReturnsSummary()
        {
            // Arrange
            var expectedApiResponse = new GetQaaDownloadSummaryQueryResponse
            {
                NewQualificationsCount = 1,
                ExtendedQualificationsCount = 2,
                DiscontinuedQualificationsCount = 3
            };

            _mockApiClient
                .Setup(x => x.Get<GetQaaDownloadSummaryQueryResponse>(
                    It.IsAny<GetQaaDownloadSummaryApiRequest>()))
                .ReturnsAsync(expectedApiResponse);

            // Act
            var result = await _handler.Handle(new GetQaaDownloadSummaryQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(1, result.Value!.NewQualificationsCount);
            Assert.Equal(2, result.Value.ExtendedQualificationsCount);
            Assert.Equal(3, result.Value.DiscontinuedQualificationsCount);
        }

        [Fact]
        public async Task Handle_WhenApiThrowsException_ShouldReturnFailureWithExceptionMessage()
        {
            // Arrange
            var exceptionMessage = "API failure";

            _mockApiClient
                .Setup(x => x.Get<GetQaaDownloadSummaryQueryResponse>(
                    It.IsAny<GetQaaDownloadSummaryApiRequest>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(new GetQaaDownloadSummaryQuery(), CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exceptionMessage, result.ErrorMessage);
        }
    }
}
