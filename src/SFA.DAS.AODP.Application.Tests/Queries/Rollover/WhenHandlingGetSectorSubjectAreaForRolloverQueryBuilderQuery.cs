namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover;

public class WhenHandlingGetSectorSubjectAreaForRolloverQueryBuilderQuery : UnitTest
{
    private readonly Mock<IApiClient> _apiClientMock = new();
    private readonly GetSectorSubjectAreaForRolloverQueryBuilderQueryHandler _handler;

    public WhenHandlingGetSectorSubjectAreaForRolloverQueryBuilderQuery()
    {
        _handler = new GetSectorSubjectAreaForRolloverQueryBuilderQueryHandler(_apiClientMock.Object);
    }

    [Fact]
    public async Task Handle_WhenApiReturnsSectorSubjectAreas_ShouldPostFiltersAndMapKnownAndUnknownCodes()
    {
        // Arrange
        var filters = RolloverQueryBuilderRequestMapper.ForSectorSubjectAreaFilter(
            new QueryBuilderFilters().SetTypes([QualificationType.GCEAlevel]));
        _apiClientMock
            .Setup(client => client.PostWithResponseCode<GetSectorSubjectAreaForRolloverQueryBuilderQueryResponse>(
                It.Is<GetSectorSubjectAreaForRolloverQueryBuilderApiRequest>(request =>
                    (RolloverQueryBuilderSectorSubjectAreaRequest)request.Data == filters)))
            .ReturnsAsync(new GetSectorSubjectAreaForRolloverQueryBuilderQueryResponse
            {
                SectorSubjectAreas =
                [SectorSubjectArea.Engineering, new SectorSubjectArea("123.4", "API value")]
            });

        // Act
        var result = await _handler.Handle(
            new GetSectorSubjectAreaForRolloverQueryBuilderQuery(filters), CancellationToken);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.SectorSubjectAreas.ShouldBe(
            [SectorSubjectArea.Engineering, SectorSubjectArea.NotSpecified]);
    }

    [Fact]
    public async Task Handle_WhenApiReturnsNull_ShouldReturnSuccessfulEmptyResponse()
    {
        // Arrange
        var filters = RolloverQueryBuilderRequestMapper.ForSectorSubjectAreaFilter(new QueryBuilderFilters());
        _apiClientMock
            .Setup(client => client.PostWithResponseCode<GetSectorSubjectAreaForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetSectorSubjectAreaForRolloverQueryBuilderApiRequest>()))
            .ReturnsAsync((GetSectorSubjectAreaForRolloverQueryBuilderQueryResponse?)null);

        // Act
        var result = await _handler.Handle(
            new GetSectorSubjectAreaForRolloverQueryBuilderQuery(filters), CancellationToken);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.SectorSubjectAreas.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenApiThrows_ShouldReturnFailedResponse()
    {
        // Arrange
        var filters = RolloverQueryBuilderRequestMapper.ForSectorSubjectAreaFilter(new QueryBuilderFilters());
        _apiClientMock
            .Setup(client => client.PostWithResponseCode<GetSectorSubjectAreaForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetSectorSubjectAreaForRolloverQueryBuilderApiRequest>()))
            .ThrowsAsync(new InvalidOperationException("API failed"));

        // Act
        var result = await _handler.Handle(
            new GetSectorSubjectAreaForRolloverQueryBuilderQuery(filters), CancellationToken);

        // Assert
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("API failed");
    }
}
