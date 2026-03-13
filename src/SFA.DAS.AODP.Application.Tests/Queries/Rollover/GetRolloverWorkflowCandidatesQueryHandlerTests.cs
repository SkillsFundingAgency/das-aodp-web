using Moq;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover;

public class GetRolloverWorkflowCandidatesQueryHandlerTests
{
    private readonly Mock<IApiClient> _mockApiClient;
    private GetRolloverWorkflowCandidatesCountQueryHandler _handler = null!;

    public GetRolloverWorkflowCandidatesQueryHandlerTests()
    {
        _mockApiClient = new Mock<IApiClient>();
        _handler = new GetRolloverWorkflowCandidatesCountQueryHandler(_mockApiClient.Object);
    }

    [Fact]
    public async Task Handle_WhenApiReturnsSuccessfulResponse_ShouldReturnSuccessAndTotalRecords()
    {
        // Arrange
        var apiResponse = new BaseMediatrResponse<GetRolloverWorkflowCandidatesCountQueryResponse>
        {
            Value = new GetRolloverWorkflowCandidatesCountQueryResponse { TotalRecords = 7 },
            Success = true
        };

        _mockApiClient
            .Setup(c => c.Get<BaseMediatrResponse<GetRolloverWorkflowCandidatesCountQueryResponse>>(It.IsAny<IGetApiRequest>()))
            .ReturnsAsync(apiResponse);

        var query = new GetRolloverWorkflowCandidatesCountQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal(7, result.Value.TotalRecords);

        _mockApiClient.Verify(c => c.Get<BaseMediatrResponse<GetRolloverWorkflowCandidatesCountQueryResponse>>(It.IsAny<IGetApiRequest>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenApiThrowsException_ShouldReturnFailureWithExceptionMessage()
    {
        // Arrange
        var ex = new InvalidOperationException("api failure");
        _mockApiClient
            .Setup(c => c.Get<BaseMediatrResponse<GetRolloverWorkflowCandidatesCountQueryResponse>>(It.IsAny<IGetApiRequest>()))
            .ThrowsAsync(ex);

        var query = new GetRolloverWorkflowCandidatesCountQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("api failure", result.ErrorMessage);

        _mockApiClient.Verify(c => c.Get<BaseMediatrResponse<GetRolloverWorkflowCandidatesCountQueryResponse>>(It.IsAny<IGetApiRequest>()), Times.Once);
    }

}