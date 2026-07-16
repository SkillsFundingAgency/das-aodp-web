using Moq;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Models.Qualifications;
using Shouldly;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover;

public class WhenHandlingGetAwardingOrganisationsForRolloverQueryBuilderQuery
{
    private readonly Mock<IApiClient> _apiClientMock = new();
    private readonly GetAwardingOrganisationsForRolloverQueryBuilderQueryHandler _handler;

    public WhenHandlingGetAwardingOrganisationsForRolloverQueryBuilderQuery()
    {
        _handler = new GetAwardingOrganisationsForRolloverQueryBuilderQueryHandler(_apiClientMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldPostLeanFilterRequestAndReturnAwardingOrganisations()
    {
        // Arrange
        var filters = RolloverQueryBuilderRequestMapper.ForAwardingOrganisationFilter(new QueryBuilderFilters());

        var expectedResponse = new GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse
        {
            AwardingOrganisations =
            [
                new AwardingOrganisation { Id = Guid.NewGuid(), NameOfqual = "Awarding organisation" }
            ]
        };

        _apiClientMock
            .Setup(a => a.PostWithResponseCode<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetAwardingOrganisationsForRolloverQueryBuilderApiRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(
            new GetAwardingOrganisationsForRolloverQueryBuilderQuery(filters),
            CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.AwardingOrganisations.ShouldBe(expectedResponse.AwardingOrganisations);
        _apiClientMock.Verify(a => a.PostWithResponseCode<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>(
            It.Is<GetAwardingOrganisationsForRolloverQueryBuilderApiRequest>(r =>
                (RolloverQueryBuilderAwardingOrganisationsRequest)r.Data == filters)), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenApiReturnsNull_ShouldReturnSuccessfulEmptyResponse()
    {
        // Arrange
        var filters = RolloverQueryBuilderRequestMapper.ForAwardingOrganisationFilter(new QueryBuilderFilters());
        _apiClientMock
            .Setup(client => client.PostWithResponseCode<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetAwardingOrganisationsForRolloverQueryBuilderApiRequest>()))
            .ReturnsAsync((GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse?)null);

        // Act
        var result = await _handler.Handle(
            new GetAwardingOrganisationsForRolloverQueryBuilderQuery(filters), CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        result.Value.AwardingOrganisations.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenApiThrows_ShouldReturnFailedResponse()
    {
        // Arrange
        var filters = RolloverQueryBuilderRequestMapper.ForAwardingOrganisationFilter(new QueryBuilderFilters());
        _apiClientMock
            .Setup(client => client.PostWithResponseCode<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>(
                It.IsAny<GetAwardingOrganisationsForRolloverQueryBuilderApiRequest>()))
            .ThrowsAsync(new InvalidOperationException("API failed"));

        // Act
        var result = await _handler.Handle(
            new GetAwardingOrganisationsForRolloverQueryBuilderQuery(filters), CancellationToken.None);

        // Assert
        result.Success.ShouldBeFalse();
        result.ErrorMessage.ShouldBe("API failed");
    }
}
