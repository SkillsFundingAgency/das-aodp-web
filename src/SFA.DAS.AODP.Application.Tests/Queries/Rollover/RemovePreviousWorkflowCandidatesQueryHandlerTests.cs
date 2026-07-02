using Moq;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover;

public class RemovePreviousWorkflowCandidatesQueryHandlerTests
{
    private readonly Mock<IApiClient> _mockApiClient;
    private RemovePreviousWorkflowCandidatesQueryHandler _handler = null!;

    public RemovePreviousWorkflowCandidatesQueryHandlerTests()
    {
        _mockApiClient = new Mock<IApiClient>();
        _handler = new RemovePreviousWorkflowCandidatesQueryHandler(_mockApiClient.Object);
    }

    [Fact]
    public async Task Handle_WhenApiCallSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        var apiResponse = new RemovePreviousWorkflowCandidatesQueryResponse();

        _mockApiClient
            .Setup(c => c.Get<RemovePreviousWorkflowCandidatesQueryResponse>(It.IsAny<IGetApiRequest>()))
            .ReturnsAsync(apiResponse);

        var query = new RemovePreviousWorkflowCandidatesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);

        _mockApiClient.Verify(c => c.Get<RemovePreviousWorkflowCandidatesQueryResponse>(It.IsAny<IGetApiRequest>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenApiThrowsException_ShouldReturnFailureWithExceptionMessage()
    {
        // Arrange
        var ex = new InvalidOperationException("api failure");
        _mockApiClient
            .Setup(c => c.Get<RemovePreviousWorkflowCandidatesQueryResponse>(It.IsAny<IGetApiRequest>()))
            .ThrowsAsync(ex);

        var query = new RemovePreviousWorkflowCandidatesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("api failure", result.ErrorMessage);

        _mockApiClient.Verify(c => c.Get<RemovePreviousWorkflowCandidatesQueryResponse>(It.IsAny<IGetApiRequest>()), Times.Once);
    }
}
