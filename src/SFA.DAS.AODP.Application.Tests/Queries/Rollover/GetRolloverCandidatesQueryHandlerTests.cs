using Moq;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover
{
    public class GetRolloverCandidatesQueryHandlerTests
    {
        private readonly Mock<IApiClient> _mockApiClient;
        private GetRolloverCandidatesQueryHandler _handler = null!;

        public GetRolloverCandidatesQueryHandlerTests()
        {
            _mockApiClient = new Mock<IApiClient>();
            _handler = new GetRolloverCandidatesQueryHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenApiReturnsData()
        {
            // Arrange
            var rolloverCandidates = new List<RolloverCandidate>
            {
                new RolloverCandidate
                {
                    Id = Guid.NewGuid(),
                    QualificationVersionId = Guid.NewGuid(),
                    FundingOfferId = Guid.NewGuid(),
                    FundingOfferName = "Funding A",
                    QualificationNumber = "QAN-001",
                    AcademicYear = "2024/25"
                }
            };

            var expectedResponse = new BaseMediatrResponse<GetRolloverCandidatesQueryResponse>
            {
                Value = new GetRolloverCandidatesQueryResponse
                {
                    RolloverCandidates = rolloverCandidates
                },
                Success = true
            };

            _mockApiClient
                .Setup(x => x.Get<GetRolloverCandidatesQueryResponse>(It.IsAny<GetRolloverCandidatesApiRequest>()))
                .ReturnsAsync(expectedResponse.Value);

            // Act
            var result = await _handler.Handle(new GetRolloverCandidatesQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.Equal(rolloverCandidates, result.Value!.RolloverCandidates);
        }

        [Fact]
        public async Task Handle_WhenApiThrowsException_ShouldReturnFailureWithExceptionMessage()
        {
            // Arrange
            var exceptionMessage = "Boom!";

            _mockApiClient
                .Setup(c => c.Get<GetRolloverCandidatesQueryResponse>(
                    It.IsAny<GetRolloverCandidatesApiRequest>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(new GetRolloverCandidatesQuery(), CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(exceptionMessage, result.ErrorMessage);

            Assert.NotNull(result.Value);
            Assert.Empty(result.Value!.RolloverCandidates);
        }
    }
}
