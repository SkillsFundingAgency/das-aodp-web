using Moq;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover;

public class GetRolloverWorkflowCandidatesQueryHandlerTests
{
    private readonly Mock<IApiClient> _apiClientMock;

    public GetRolloverWorkflowCandidatesQueryHandlerTests()
    {
        _apiClientMock = new Mock<IApiClient>();
    }

    [Fact]
    public async Task Handle_WhenApiClientReturnsValue_ResponseIsSuccessfulAndValueIsPreserved()
    {
        // Arrange
        var expected = new GetRolloverWorkflowCandidatesQueryResponse
        {
            Data = new List<RolloverWorkflowCandidate>
                {
                    new RolloverWorkflowCandidate { Id = Guid.NewGuid() }
                },
            Skip = 0,
            Take = 0,
            TotalRecords = 1
        };

        _apiClientMock
            .Setup(c => c.Get<GetRolloverWorkflowCandidatesQueryResponse>(It.IsAny<IGetApiRequest>()))
            .ReturnsAsync(expected);

        var handler = new GetRolloverWorkflowCandidatesQueryHandler(_apiClientMock.Object);

        // Act
        var result = await handler.Handle(new GetRolloverWorkflowCandidatesQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Same(expected, result.Value);
        _apiClientMock.Verify(c => c.Get<GetRolloverWorkflowCandidatesQueryResponse>(It.IsAny<IGetApiRequest>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenApiClientThrows_ResponseIsNotSuccessfulAndErrorMessageSet()
    {
        // Arrange
        var ex = new Exception("api failure");
        _apiClientMock
            .Setup(c => c.Get<GetRolloverWorkflowCandidatesQueryResponse>(It.IsAny<IGetApiRequest>()))
            .ThrowsAsync(ex);

        var handler = new GetRolloverWorkflowCandidatesQueryHandler(_apiClientMock.Object);

        // Act
        var result = await handler.Handle(new GetRolloverWorkflowCandidatesQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("api failure", result.ErrorMessage);
        // Value should be default (new) as per BaseMediatrResponse generic initialization
        Assert.NotNull(result.Value);
        _apiClientMock.Verify(c => c.Get<GetRolloverWorkflowCandidatesQueryResponse>(It.IsAny<IGetApiRequest>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsApiClient_WithExpectedGetRequestParameters()
    {
        // Arrange
        GetRolloverWorkflowCandidatesApiRequest? capturedRequest = null;

        var returned = new GetRolloverWorkflowCandidatesQueryResponse();
        _apiClientMock
            .Setup(c => c.Get<GetRolloverWorkflowCandidatesQueryResponse>(It.IsAny<IGetApiRequest>()))
            .Callback<IGetApiRequest>(req =>
            {
                // capture strongly-typed request for assertions
                capturedRequest = req as GetRolloverWorkflowCandidatesApiRequest;
            })
            .ReturnsAsync(returned);

        var handler = new GetRolloverWorkflowCandidatesQueryHandler(_apiClientMock.Object);

        // Act
        var result = await handler.Handle(new GetRolloverWorkflowCandidatesQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        // Handler constructs the request with (0, 0) so Skip and Take should be 0 (not null)
        Assert.Equal(0, capturedRequest!.Skip);
        Assert.Equal(0, capturedRequest.Take);
    }
}