namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover;

public class WhenHandlingGetQualificationVersionsForRolloverQueryBuilderQuery : UnitTest
{
    private readonly Mock<IApiClient> _apiClientMock = new();
    private readonly GetQualificationVersionsForRolloverQueryBuilderQueryHandler _handler;

    public WhenHandlingGetQualificationVersionsForRolloverQueryBuilderQuery()
    {
        _handler = new GetQualificationVersionsForRolloverQueryBuilderQueryHandler(_apiClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldPostAllSelectedFilterIdsAndReturnQualificationVersions()
    {
        // Arrange
        var filters = new RolloverQueryBuilderRequest(
            LevelIds: [1, 2],
            TypeIds: [3, 4],
            SectorSubjectAreaIds: ["01", "02"],
            AwardingOrganisationIds: ["AO1", "AO2"]);

        var expectedResponse = new GetQualificationVersionsForRolloverQueryBuilderQueryResponse
        {
            QualificationVersions =
            [
                new RolloverQueryBuilderCandidatesDto
                {
                    Id = Guid.NewGuid(),
                    QualificationNumber = "123/4567/8",
                    QualificationName = "Qualification",
                }
            ]
        };

        _apiClientMock
            .Setup(a => a.PostWithResponseCodeAsMultipart<GetQualificationVersionsForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetQualificationVersionsForRolloverQueryBuilderApiRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(
            new GetQualificationVersionsForRolloverQueryBuilderQuery(filters),
            CancellationToken);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.QualificationVersions.ShouldBe(expectedResponse.QualificationVersions);
        _apiClientMock.Verify(a => a.PostWithResponseCodeAsMultipart<GetQualificationVersionsForRolloverQueryBuilderQueryResponse>(
            It.Is<GetQualificationVersionsForRolloverQueryBuilderApiRequest>(r => r.Data == filters)), Times.Once);
    }

    [Fact]
    public void ApiRequest_WhenFiltersAreProvided_MapsEveryValueToMultipartFormData()
    {
        // Arrange
        var filters = new RolloverQueryBuilderRequest(
            LevelIds: [1, 2],
            TypeIds: [3, 4],
            SectorSubjectAreaIds: ["01", "02"],
            AwardingOrganisationIds: ["AO1", "AO2"]);
        var request = new GetQualificationVersionsForRolloverQueryBuilderApiRequest(filters);
        KeyValuePair<string, string>[] expected =
        [
            new("LevelIds", "1"),
            new("LevelIds", "2"),
            new("TypeIds", "3"),
            new("TypeIds", "4"),
            new("SectorSubjectAreaIds", "01"),
            new("SectorSubjectAreaIds", "02"),
            new("AwardingOrganisationIds", "AO1"),
            new("AwardingOrganisationIds", "AO2")
        ];

        // Act
        var result = request.FormData.ToArray();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public async Task Handle_WhenApiThrowsException_ShouldThrowException()
    {
        // Arrange
        const string exceptionMessage = "API failed";
        var filters = RolloverQueryBuilderRequestMapper.ForAll(new QueryBuilderFilters());
        var exception = new Exception(exceptionMessage);

        _apiClientMock
            .Setup(a => a.PostWithResponseCodeAsMultipart<GetQualificationVersionsForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetQualificationVersionsForRolloverQueryBuilderApiRequest>()))
            .ThrowsAsync(exception);

        // Act
        var result = await Should.ThrowAsync<Exception>(() => _handler.Handle(
            new GetQualificationVersionsForRolloverQueryBuilderQuery(filters),
            CancellationToken));

        // Assert
        result.ShouldBe(exception);
    }
}
