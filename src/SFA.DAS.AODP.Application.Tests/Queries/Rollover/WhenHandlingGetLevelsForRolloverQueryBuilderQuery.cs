namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover;

public class WhenHandlingGetLevelsForRolloverQueryBuilderQuery : UnitTest
{
    private readonly Mock<IApiClient> _apiClientMock = new();
    private readonly GetLevelsForRolloverQueryBuilderQueryHandler _handler;

    public WhenHandlingGetLevelsForRolloverQueryBuilderQuery()
    {
        _handler = new GetLevelsForRolloverQueryBuilderQueryHandler(_apiClientMock.Object);
    }

    [Fact]
    public async Task Handle_WhenApiReturnsLevels_ShouldMapKnownAndUnknownLevelIds()
    {
        // Arrange
        _apiClientMock
            .Setup(client => client.Get<GetLevelsForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetLevelsForRolloverQueryBuilderApiRequest>()))
            .ReturnsAsync(new GetLevelsForRolloverQueryBuilderQueryResponse
            {
                Levels = [QualificationLevel.Level3, new QualificationLevel(123, "API value")]
            });

        // Act
        var result = await _handler.Handle(new GetLevelsForRolloverQueryBuilderQuery(), CancellationToken);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.Levels.ShouldBe([QualificationLevel.Level3, QualificationLevel.Unspecified]);
    }

    [Fact]
    public async Task Handle_WhenApiThrows_ShouldReturnFailedResponse()
    {
        // Arrange
        _apiClientMock
            .Setup(client => client.Get<GetLevelsForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetLevelsForRolloverQueryBuilderApiRequest>()))
            .ThrowsAsync(new InvalidOperationException("API failed"));

        // Act
        var result = await _handler.Handle(new GetLevelsForRolloverQueryBuilderQuery(), CancellationToken);

        // Assert
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("API failed");
    }
}
