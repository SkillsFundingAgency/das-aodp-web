using Moq;
using SFA.DAS.AODP.Application.Queries.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover
{
    public class GetRolloverCandidatesForExportQueryHandlerTests
    {
        private readonly Mock<IApiClient> _mockApiClient;
        private GetRolloverCandidatesForExportQueryHandler _handler = null!;

        public GetRolloverCandidatesForExportQueryHandlerTests()
        {
            _mockApiClient = new Mock<IApiClient>();
            _handler = new GetRolloverCandidatesForExportQueryHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenApiReturnsFile()
        {
            // Arrange
            var workflowRunId = Guid.NewGuid();

            var expectedApiResponse = new GetRolloverCandidatesForExportQueryResponse
            {
                FileContent = new byte[] { 1, 2, 3 },
                FileName = "export.csv",
                ContentType = "text/csv"
            };

            _mockApiClient
                .Setup(x => x.Get<GetRolloverCandidatesForExportQueryResponse>(
                    It.IsAny<GetRolloverCandidatesForExportApiRequest>()))
                .ReturnsAsync(expectedApiResponse);

            var query = new GetRolloverCandidatesForExportQuery
            {
                RolloverWorkflowRunId = workflowRunId
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);

            Assert.Equal(expectedApiResponse.FileName, result.Value!.FileName);
            Assert.Equal(expectedApiResponse.ContentType, result.Value!.ContentType);
            Assert.Equal(expectedApiResponse.FileContent, result.Value!.FileContent);
        }

        [Fact]
        public async Task Handle_WhenApiThrowsException_ShouldReturnFailureWithExceptionMessage()
        {
            // Arrange
            var workflowRunId = Guid.NewGuid();
            var exceptionMessage = "API failure";

            _mockApiClient
                .Setup(x => x.Get<GetRolloverCandidatesForExportQueryResponse>(
                    It.IsAny<GetRolloverCandidatesForExportApiRequest>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            var query = new GetRolloverCandidatesForExportQuery
            {
                RolloverWorkflowRunId = workflowRunId
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exceptionMessage, result.ErrorMessage);

            Assert.NotNull(result.Value);
            Assert.Empty(result.Value!.FileContent);
            Assert.Equal(string.Empty, result.Value!.FileName);
            Assert.Equal("text/csv", result.Value!.ContentType);
        }
    }
}
