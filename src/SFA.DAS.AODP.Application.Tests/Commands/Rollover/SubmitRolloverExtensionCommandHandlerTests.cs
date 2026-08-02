using Moq;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.UnitTests.Commands.Rollover
{
    public class SubmitRolloverExtensionCommandHandlerTests : UnitTest
    {
        private readonly Mock<IApiClient> _mockApiClient;
        private readonly SubmitRolloverExtensionCommandHandler _handler;

        public SubmitRolloverExtensionCommandHandlerTests()
        {
            _mockApiClient = new Mock<IApiClient>();
            _handler = new SubmitRolloverExtensionCommandHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenApiReturnsResponse()
        {
            // Arrange
            var request = new SubmitRolloverExtensionCommand
            {
                Items = []
            };

            var resultMessageText = "Rollover extension submitted successfully.";

            var expectedApiResponse = new SubmitRolloverExtensionCommandResponse
            {
                ResultMessage = resultMessageText
            };

            _mockApiClient
                .Setup(c => c.PostWithResponseCodeAsJsonFile<SubmitRolloverExtensionCommandResponse>(
                    It.IsAny<SubmitRolloverExtensionApiRequest>()))
                .ReturnsAsync(expectedApiResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedApiResponse.ResultMessage, result.Value.ResultMessage);
            Assert.Null(result.ErrorMessage);

            _mockApiClient.Verify(c =>
                c.PostWithResponseCodeAsJsonFile<SubmitRolloverExtensionCommandResponse>(
                    It.IsAny<SubmitRolloverExtensionApiRequest>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenApiThrowsException()
        {
            // Arrange
            var request = new SubmitRolloverExtensionCommand();

            _mockApiClient
                .Setup(c => c.PostWithResponseCodeAsJsonFile<SubmitRolloverExtensionCommandResponse>(
                    It.IsAny<SubmitRolloverExtensionApiRequest>()))
                .ThrowsAsync(new Exception("API failed"));

            // Act
            var result = await _handler.Handle(request, CancellationToken);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("API failed", result.ErrorMessage);

            _mockApiClient.Verify(c =>
                c.PostWithResponseCodeAsJsonFile<SubmitRolloverExtensionCommandResponse>(
                    It.IsAny<SubmitRolloverExtensionApiRequest>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SendsCorrectApiRequest()
        {
            // Arrange
            var request = new SubmitRolloverExtensionCommand
            {
                Items = []
            };

            SubmitRolloverExtensionApiRequest? captured = null;

            _mockApiClient
                .Setup(c => c.PostWithResponseCodeAsJsonFile<SubmitRolloverExtensionCommandResponse>(
                    It.IsAny<IPostMultipartJsonFileApiRequest>()))
                .Callback<IPostMultipartJsonFileApiRequest>(req =>
                {
                    captured = req as SubmitRolloverExtensionApiRequest;
                })
                .ReturnsAsync(new SubmitRolloverExtensionCommandResponse());

            // Act
            await _handler.Handle(request, CancellationToken);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(request, captured!.Data);
        }

    }
}
