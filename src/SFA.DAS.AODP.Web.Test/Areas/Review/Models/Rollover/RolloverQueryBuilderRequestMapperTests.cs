using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;

namespace SFA.DAS.AODP.Web.Test.Areas.Review.Models.Rollover;

public class RolloverQueryBuilderRequestMapperTests
{
    [Fact]
    public void QueryBuilderFilters_ShouldReturnNewInstanceWhenUpdated()
    {
        // Arrange
        var filters = new QueryBuilderFilters();

        // Act
        var updatedFilters = filters.SetLevels([QualificationLevel.Level3]);

        // Assert
        updatedFilters.ShouldNotBeSameAs(filters);
        filters.Levels.ShouldBeEmpty();
        updatedFilters.Levels.ShouldBe([QualificationLevel.Level3]);
    }

    [Fact]
    public void Map_ShouldConvertSelectedFiltersToLeanRequestIds()
    {
        // Arrange
        var awardingOrganisationId = Guid.NewGuid();
        var filters = new QueryBuilderFilters()
            .SetLevels([QualificationLevel.Level3])
            .SetTypes([QualificationType.FunctionalSkills])
            .SetSectorSubjectAreas([SectorSubjectArea.Engineering])
            .SetAwardingOrganisations([awardingOrganisationId], AwardingOrganisationSelectionType.SpecificSelection);

        // Act
        var result = RolloverQueryBuilderRequestMapper.Map(filters);

        // Assert
        result.LevelIds.ShouldBe([QualificationLevel.Level3.Id]);
        result.TypeIds.ShouldBe([QualificationType.FunctionalSkills.Id]);
        result.SectorSubjectAreaIds.ShouldBe([SectorSubjectArea.Engineering.Code]);
        result.AwardingOrganisationIds.ShouldBe([awardingOrganisationId]);
    }
}
