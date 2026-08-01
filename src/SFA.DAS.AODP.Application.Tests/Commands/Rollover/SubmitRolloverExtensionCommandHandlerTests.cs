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
                .Setup(c => c.PostWithResponseCodeAsMultipart<SubmitRolloverExtensionCommandResponse>(
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
                c.PostWithResponseCodeAsMultipart<SubmitRolloverExtensionCommandResponse>(
                    It.IsAny<SubmitRolloverExtensionApiRequest>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenApiThrowsException()
        {
            // Arrange
            var request = new SubmitRolloverExtensionCommand();

            _mockApiClient
                .Setup(c => c.PostWithResponseCodeAsMultipart<SubmitRolloverExtensionCommandResponse>(
                    It.IsAny<SubmitRolloverExtensionApiRequest>()))
                .ThrowsAsync(new Exception("API failed"));

            // Act
            var result = await _handler.Handle(request, CancellationToken);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("API failed", result.ErrorMessage);

            _mockApiClient.Verify(c =>
                c.PostWithResponseCodeAsMultipart<SubmitRolloverExtensionCommandResponse>(
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
                .Setup(c => c.PostWithResponseCodeAsMultipart<SubmitRolloverExtensionCommandResponse>(
                    It.IsAny<IPostMultipartFormDataApiRequest>()))
                .Callback<IPostMultipartFormDataApiRequest>(req =>
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

        [Fact]
        public void ApiRequest_WhenItemIsProvided_MapsNestedValuesToMultipartFormData()
        {
            // Arrange
            var proposedEndDate = new DateTime(2026, 7, 31, 10, 15, 0, DateTimeKind.Utc);
            var request = new SubmitRolloverExtensionApiRequest
            {
                Data = new SubmitRolloverExtensionCommand
                {
                    Items =
                    [
                        new FundingExtensionItem
                        {
                            Qan = "123/4567/8",
                            FundingStreamName = "Adult Skills",
                            RolloverStatus = "Approved",
                            ExclusionReason = "Not eligible",
                            ProposedFundingApprovalEndDate = proposedEndDate,
                            Comments = "Checked"
                        }
                    ]
                }
            };
            KeyValuePair<string, string>[] expected =
            [
                new("Items[0].Qan", "123/4567/8"),
                new("Items[0].FundingStreamName", "Adult Skills"),
                new("Items[0].RolloverStatus", "Approved"),
                new("Items[0].ExclusionReason", "Not eligible"),
                new("Items[0].ProposedFundingApprovalEndDate", "2026-07-31T10:15:00Z"),
                new("Items[0].Comments", "Checked")
            ];

            // Act
            var result = request.FormData.ToArray();

            // Assert
            result.ShouldBe(expected);
        }
    }
}
