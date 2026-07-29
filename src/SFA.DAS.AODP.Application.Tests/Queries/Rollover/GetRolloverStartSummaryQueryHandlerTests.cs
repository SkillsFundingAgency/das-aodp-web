using SFA.DAS.AODP.Application.Queries.Rollover;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover
{
    public class GetRolloverStartSummaryQueryHandlerTests
    {
        private readonly Mock<IApiClient> _mockApiClient;
        private GetRolloverStartSummaryQueryHandler _handler = null!;

        public GetRolloverStartSummaryQueryHandlerTests()
        {
            _mockApiClient = new Mock<IApiClient>();
            _handler = new GetRolloverStartSummaryQueryHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenApiReturnsData()
        {
            var expected = new GetRolloverStartSummaryQueryResponse
            {
                TotalCandidatesCount = 10,
                CandidatesEligibleCount = 4,
                CandidatesIneligibleCount = 3,
                CandidatesRemainingCount = 2
            };

            _mockApiClient
                .Setup(x => x.Get<GetRolloverStartSummaryQueryResponse>(It.IsAny<GetRolloverStartSummaryApiRequest>()))
                .ReturnsAsync(expected);

            var result = await _handler.Handle(new GetRolloverStartSummaryQuery(), CancellationToken.None);

            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(expected.TotalCandidatesCount, result.Value.TotalCandidatesCount);
            Assert.Equal(expected.CandidatesEligibleCount, result.Value.CandidatesEligibleCount);
            Assert.Equal(expected.CandidatesIneligibleCount, result.Value.CandidatesIneligibleCount);
            Assert.Equal(expected.CandidatesRemainingCount, result.Value.CandidatesRemainingCount);
        }

        [Fact]
        public async Task Handle_WhenApiThrowsException_ReturnsFailureWithMessage()
        {
            var exceptionMessage = "Boom";

            _mockApiClient
                .Setup(x => x.Get<GetRolloverStartSummaryQueryResponse>(It.IsAny<GetRolloverStartSummaryApiRequest>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            var result = await _handler.Handle(new GetRolloverStartSummaryQuery(), CancellationToken.None);

            Assert.False(result.Success);
            Assert.Equal(exceptionMessage, result.ErrorMessage);
        }
    }
}
