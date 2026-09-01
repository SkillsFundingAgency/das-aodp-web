using Moq;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.UnitTests.Commands.Rollover
{
    public class ValidateRolloverExtensionCommandHandlerTests : UnitTest
    {
        private readonly Mock<IApiClient> _mockApiClient;
        private readonly ValidateRolloverExtensionCommandHandler _handler;

        public ValidateRolloverExtensionCommandHandlerTests()
        {
            _mockApiClient = new Mock<IApiClient>();
            _handler = new ValidateRolloverExtensionCommandHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenApiReturnsResponse()
        {
            // Arrange
            var request = new ValidateRolloverExtensionCommand
            {
                RolloverCandidates = []
            };

            var expectedApiResponse = new ValidateRolloverExtensionCommandResponse
            {
                IsValid = true,
                ValidationSuccessSummary = new FundingExtensionSummary
                {
                    TotalCandidatesCount = 24,
                    CandidatesExtendedInUploadCount = 1,
                    TotalCandidatesToBeExtendedCount = 4,
                    TotalCandidatesToBeExcludedCount = 4,
                    TotalCandidatesToBeReviewedCount = 16
                }
            };

            _mockApiClient
                .Setup(c => c.PostWithResponseCodeAsJsonFile<ValidateRolloverExtensionCommandResponse>(
                    It.IsAny<ValidateRolloverExtensionApiRequest>()))
                .ReturnsAsync(expectedApiResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedApiResponse.ValidationSuccessSummary, result.Value.ValidationSuccessSummary);
            Assert.Null(result.ErrorMessage);

            _mockApiClient.Verify(c =>
                c.PostWithResponseCodeAsJsonFile<ValidateRolloverExtensionCommandResponse>(
                    It.IsAny<ValidateRolloverExtensionApiRequest>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenApiThrowsException()
        {
            // Arrange
            var request = new ValidateRolloverExtensionCommand();

            _mockApiClient
                .Setup(c => c.PostWithResponseCodeAsJsonFile<ValidateRolloverExtensionCommandResponse>(
                    It.IsAny<ValidateRolloverExtensionApiRequest>()))
                .ThrowsAsync(new Exception("API failed"));

            // Act
            var result = await _handler.Handle(request, CancellationToken);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("API failed", result.ErrorMessage);

            _mockApiClient.Verify(c =>
                c.PostWithResponseCodeAsJsonFile<ValidateRolloverExtensionCommandResponse>(
                    It.IsAny<ValidateRolloverExtensionApiRequest>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SendsCorrectApiRequest()
        {
            // Arrange
            var request = new ValidateRolloverExtensionCommand
            {
                RolloverCandidates = []
            };

            ValidateRolloverExtensionApiRequest? captured = null;

            _mockApiClient
                .Setup(c => c.PostWithResponseCodeAsJsonFile<ValidateRolloverExtensionCommandResponse>(
                    It.IsAny<IPostMultipartJsonFileApiRequest>()))
                .Callback<IPostMultipartJsonFileApiRequest>(req =>
                {
                    captured = req as ValidateRolloverExtensionApiRequest;
                })
                .ReturnsAsync(new ValidateRolloverExtensionCommandResponse());

            // Act
            await _handler.Handle(request, CancellationToken);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(request, captured!.Data);
        }

    }
}
