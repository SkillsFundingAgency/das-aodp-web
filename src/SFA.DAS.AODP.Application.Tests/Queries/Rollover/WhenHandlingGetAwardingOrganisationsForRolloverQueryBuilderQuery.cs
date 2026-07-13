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
            It.Is<GetAwardingOrganisationsForRolloverQueryBuilderApiRequest>(r => r.Data == filters)), Times.Once);
    }
}
