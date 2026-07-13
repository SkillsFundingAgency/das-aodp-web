using Moq;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;
using Shouldly;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover;

public class WhenHandlingGetQualificationVersionsForRolloverQueryBuilderQuery
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
        var awardingOrganisationId = string.Empty;
        var filters = RolloverQueryBuilderRequestMapper.ForAll(new QueryBuilderFilters());

        var expectedResponse = new GetQualificationVersionsForRolloverQueryBuilderQueryResponse
        {
            QualificationVersions =
            [
                new RolloverQualificationVersion
                {
                    Id = Guid.NewGuid(),
                    QualificationReference = "123/4567/8",
                    QualificationName = "Qualification",
                    AwardingOrganisationId = awardingOrganisationId
                }
            ]
        };

        _apiClientMock
            .Setup(a => a.PostWithResponseCode<GetQualificationVersionsForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetQualificationVersionsForRolloverQueryBuilderApiRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(
            new GetQualificationVersionsForRolloverQueryBuilderQuery(filters),
            CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.QualificationVersions.ShouldBe(expectedResponse.QualificationVersions);
        _apiClientMock.Verify(a => a.PostWithResponseCode<GetQualificationVersionsForRolloverQueryBuilderQueryResponse>(
            It.Is<GetQualificationVersionsForRolloverQueryBuilderApiRequest>(r => r.Data == filters)), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenApiThrowsException_ShouldThrowException()
    {
        // Arrange
        const string exceptionMessage = "API failed";
        var filters = RolloverQueryBuilderRequestMapper.ForAll(new QueryBuilderFilters());
        var exception = new Exception(exceptionMessage);

        _apiClientMock
            .Setup(a => a.PostWithResponseCode<GetQualificationVersionsForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetQualificationVersionsForRolloverQueryBuilderApiRequest>()))
            .ThrowsAsync(exception);

        // Act
        var result = await Should.ThrowAsync<Exception>(() => _handler.Handle(
            new GetQualificationVersionsForRolloverQueryBuilderQuery(filters),
            CancellationToken.None));

        // Assert
        result.ShouldBe(exception);
    }
}
