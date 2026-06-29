using SFA.DAS.AODP.Domain.Rollover;
using Shouldly;

namespace SFA.DAS.AODP.Application.UnitTests.Queries.Rollover;

public class RolloverQueryBuilderRequestBuilderTests
{
    [Fact]
    public void Build_ShouldKeepOnlyDistinctIdsAndOmitEmptyValues()
    {
        // Arrange
        var awardingOrganisationId = Guid.NewGuid();

        // Act
        var result = RolloverQueryBuilderRequest.Builder()
            .WithLevels([3, 3, -1])
            .WithTypes([7, 0, 7])
            .WithSectorSubjectAreas(["4.1", "", "4.1"])
            .WithAwardingOrganisations([awardingOrganisationId, Guid.Empty, awardingOrganisationId])
            .Build();

        // Assert
        result.LevelIds.ShouldBe([3]);
        result.TypeIds.ShouldBe([7]);
        result.SectorSubjectAreaIds.ShouldBe(["4.1"]);
        result.AwardingOrganisationIds.ShouldBe([awardingOrganisationId]);
    }
}
