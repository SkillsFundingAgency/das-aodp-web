using Azure;
using Moq;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Infrastructure.ApiClient;

namespace SFA.DAS.AODP.Application.UnitTests.Commands.Rollover
{
    public class WhenHandlingCreateRolloverWorkflowRunCommandHandlerTests
    {
        private readonly Mock<IApiClient> _mockApiClient;
        private CreateRolloverWorkflowRunCommandHandler _handler = null!;

        public WhenHandlingCreateRolloverWorkflowRunCommandHandlerTests()
        {
            _mockApiClient = new Mock<IApiClient>();
            _handler = new CreateRolloverWorkflowRunCommandHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenApiReturnsResponse()
        {
            // Arrange
            var request = new CreateRolloverWorkflowRunCommand
            {
                AcademicYear = "2024/25"
            };

            var expectedApiResponse = new CreateRolloverWorkflowRunCommandResponse
            {
                RolloverWorkflowRunId = Guid.NewGuid()
            };

            _mockApiClient
                .Setup(c => c.PostWithResponseCode<CreateRolloverWorkflowRunCommandResponse>(
                    It.IsAny<CreateRolloverWorkflowRunApiRequest>()))
                .ReturnsAsync(expectedApiResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert

            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedApiResponse.RolloverWorkflowRunId, result.Value!.RolloverWorkflowRunId);
            Assert.Null(result.ErrorMessage);

            _mockApiClient.Verify(c =>
                c.PostWithResponseCode<CreateRolloverWorkflowRunCommandResponse>(
                    It.IsAny<CreateRolloverWorkflowRunApiRequest>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenApiThrowsException_ShouldReturnFailureWithExceptionMessage()
        {
            // Arrange
            var request = new CreateRolloverWorkflowRunCommand();

            _mockApiClient
                .Setup(c => c.PostWithResponseCode<CreateRolloverWorkflowRunCommandResponse>(
                    It.IsAny<CreateRolloverWorkflowRunApiRequest>()))
                .ThrowsAsync(new Exception("API failed"));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("API failed", result.ErrorMessage);
            Assert.Null(result.Value);
        }
    }
}
