using Moq;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Domain.ValueObjects;
using Shouldly;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover;

public class WhenHandlingGetTypesForRolloverQueryBuilderQuery
{
    private readonly Mock<IApiClient> _apiClientMock = new();
    private readonly GetTypesForRolloverQueryBuilderQueryHandler _handler;

    public WhenHandlingGetTypesForRolloverQueryBuilderQuery()
    {
        _handler = new GetTypesForRolloverQueryBuilderQueryHandler(_apiClientMock.Object);
    }

    [Fact]
    public async Task Handle_WhenApiReturnsTypes_ShouldPostFiltersAndMapKnownAndUnknownTypeIds()
    {
        // Arrange
        var filters = RolloverQueryBuilderRequestMapper.ForTypesFilter(
            new QueryBuilderFilters().SetLevels([QualificationLevel.Level3]));
        _apiClientMock
            .Setup(client => client.PostWithResponseCode<GetTypesForRolloverQueryBuilderQueryResponse>(
                It.Is<GetTypesForRolloverQueryBuilderApiRequest>(request =>
                    (RolloverQueryBuilderTypesRequest)request.Data == filters)))
            .ReturnsAsync(new GetTypesForRolloverQueryBuilderQueryResponse
            {
                Types = [QualificationType.GCEAlevel, new QualificationType(123, "API value")]
            });

        // Act
        var result = await _handler.Handle(
            new GetTypesForRolloverQueryBuilderQuery(filters), CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.Types.ShouldBe([QualificationType.GCEAlevel, QualificationType.Unknown]);
    }

    [Fact]
    public async Task Handle_WhenApiReturnsNull_ShouldReturnSuccessfulEmptyResponse()
    {
        // Arrange
        var filters = RolloverQueryBuilderRequestMapper.ForTypesFilter(new QueryBuilderFilters());
        _apiClientMock
            .Setup(client => client.PostWithResponseCode<GetTypesForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetTypesForRolloverQueryBuilderApiRequest>()))
            .ReturnsAsync((GetTypesForRolloverQueryBuilderQueryResponse?)null);

        // Act
        var result = await _handler.Handle(
            new GetTypesForRolloverQueryBuilderQuery(filters), CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.Types.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenApiThrows_ShouldReturnFailedResponse()
    {
        // Arrange
        var filters = RolloverQueryBuilderRequestMapper.ForTypesFilter(new QueryBuilderFilters());
        _apiClientMock
            .Setup(client => client.PostWithResponseCode<GetTypesForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetTypesForRolloverQueryBuilderApiRequest>()))
            .ThrowsAsync(new InvalidOperationException("API failed"));

        // Act
        var result = await _handler.Handle(
            new GetTypesForRolloverQueryBuilderQuery(filters), CancellationToken.None);

        // Assert
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("API failed");
    }
}
