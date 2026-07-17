using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Domain.ValueObjects;
using RolloverQueryBuilderRequestMapper = SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.RolloverQueryBuilderRequestMapper;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models.Rollover;

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
        var awardingOrganisation = "RN1234";
        var originalAwardingOrganisation = new AwardingOrganisation
        {
            RecognitionNumber = "RN1234"
        };

        var filters = new QueryBuilderFilters()
            .SetLevels([QualificationLevel.Level3])
            .SetTypes([QualificationType.FunctionalSkills])
            .SetSectorSubjectAreas([SectorSubjectArea.Engineering], [SectorSubjectArea.Engineering], SectorSubjectAreaSelectionType.None)
            .SetAwardingOrganisations([awardingOrganisation], [originalAwardingOrganisation], AwardingOrganisationSelectionType.SpecificSelection);

        // Act
        var result = RolloverQueryBuilderRequestMapper.ForAll(filters);

        // Assert
        result.LevelIds.ShouldBe([QualificationLevel.Level3.Id]);
        result.TypeIds.ShouldBe([QualificationType.FunctionalSkills.Id]);
        result.SectorSubjectAreaIds.ShouldBe([SectorSubjectArea.Engineering.Code]);
        result.AwardingOrganisationIds.ShouldBe(["RN1234"]);
    }
}
