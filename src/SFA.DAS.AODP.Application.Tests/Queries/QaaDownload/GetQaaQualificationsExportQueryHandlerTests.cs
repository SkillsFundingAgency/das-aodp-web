using Moq;
using SFA.DAS.AODP.Application.Queries.QaaDownload;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.QaaDownload;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.QaaDownload
{
    public class GetQaaQualificationsExportQueryHandlerTests
    {
        private readonly Mock<IApiClient> _mockApiClient;
        private readonly GetQaaQualificationsExportQueryHandler _handler;

        public GetQaaQualificationsExportQueryHandlerTests()
        {
            _mockApiClient = new Mock<IApiClient>();
            _handler = new GetQaaQualificationsExportQueryHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenApiReturnsFile()
        {
            // Arrange
            var expectedApiResponse = new GetQaaQualificationsExportQueryResponse
            {
                FileContent = new byte[] { 1, 2, 3 },
                FileName = "export.csv",
                ContentType = "text/csv"
            };

            _mockApiClient
                .Setup(x => x.Get<GetQaaQualificationsExportQueryResponse>(
                    It.Is<GetQaaQualificationsExportApiRequest>(r => r.CurrentUsername == "tester")))
                .ReturnsAsync(expectedApiResponse);

            var query = new GetQaaQualificationsExportQuery { CurrentUsername = "tester" };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedApiResponse.FileName, result.Value!.FileName);
            Assert.Equal(expectedApiResponse.ContentType, result.Value.ContentType);
            Assert.Equal(expectedApiResponse.FileContent, result.Value.FileContent);
        }

        [Fact]
        public async Task Handle_WhenApiThrowsException_ShouldReturnFailureWithExceptionMessage()
        {
            // Arrange
            var exceptionMessage = "API failure";

            _mockApiClient
                .Setup(x => x.Get<GetQaaQualificationsExportQueryResponse>(
                    It.IsAny<GetQaaQualificationsExportApiRequest>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            var query = new GetQaaQualificationsExportQuery { CurrentUsername = "tester" };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exceptionMessage, result.ErrorMessage);
        }
    }
}
