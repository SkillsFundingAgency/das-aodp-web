using Moq;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.UnitTests.Commands.Rollover
{
    public class ValidateFundingExtensionCandidatesCommandHandlerTests
    {
        private readonly Mock<IApiClient> _mockApiClient;
        private readonly ValidateFundingExtensionCandidatesCommandHandler _handler;

        public ValidateFundingExtensionCandidatesCommandHandlerTests()
        {
            _mockApiClient = new Mock<IApiClient>();
            _handler = new ValidateFundingExtensionCandidatesCommandHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenApiReturnsResponse()
        {
            // Arrange
            var request = new ValidateFundingExtensionCandidatesCommand
            {
                FundingExtensionCandidates = []
            };

            var expectedApiResponse = new ValidateFundingExtensionCandidatesCommandResponse
            {
                TotalCandidates = 1,
                FailedCandidateCount = 0,
                IsValid = true
            };

            _mockApiClient
                .Setup(c => c.PostWithResponseCode<ValidateFundingExtensionCandidatesCommandResponse>(
                    It.IsAny<ValidateFundingExtensionCandidatesApiRequest>()))
                .ReturnsAsync(expectedApiResponse);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedApiResponse.TotalCandidates, result.Value!.TotalCandidates);
            Assert.Equal(expectedApiResponse.FailedCandidateCount, result.Value!.FailedCandidateCount);
            Assert.Equal(expectedApiResponse.IsValid, result.Value!.IsValid);
            Assert.Null(result.ErrorMessage);

            _mockApiClient.Verify(c =>
                c.PostWithResponseCode<ValidateFundingExtensionCandidatesCommandResponse>(
                    It.IsAny<ValidateFundingExtensionCandidatesApiRequest>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenApiThrowsException()
        {
            // Arrange
            var request = new ValidateFundingExtensionCandidatesCommand();

            _mockApiClient
                .Setup(c => c.PostWithResponseCode<ValidateFundingExtensionCandidatesCommandResponse>(
                    It.IsAny<ValidateFundingExtensionCandidatesApiRequest>()))
                .ThrowsAsync(new Exception("API failed"));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("API failed", result.ErrorMessage);

            _mockApiClient.Verify(c =>
                c.PostWithResponseCode<ValidateFundingExtensionCandidatesCommandResponse>(
                    It.IsAny<ValidateFundingExtensionCandidatesApiRequest>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_SendsCorrectApiRequest()
        {
            // Arrange
            var request = new ValidateFundingExtensionCandidatesCommand
            {
                FundingExtensionCandidates = []
            };

            ValidateFundingExtensionCandidatesApiRequest? captured = null;

            _mockApiClient
                .Setup(c => c.PostWithResponseCode<ValidateFundingExtensionCandidatesCommandResponse>(
                    It.IsAny<IPostApiRequest>()))
                .Callback<IPostApiRequest>(req =>
                {
                    captured = req as ValidateFundingExtensionCandidatesApiRequest;
                })
                .ReturnsAsync(new ValidateFundingExtensionCandidatesCommandResponse());

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(request, captured!.Data);
        }

    }
}
