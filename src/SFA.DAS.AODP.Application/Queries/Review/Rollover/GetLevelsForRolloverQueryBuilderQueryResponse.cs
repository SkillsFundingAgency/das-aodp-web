namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public record GetLevelsForRolloverQueryBuilderQueryResponse
{
    public IEnumerable<QualificationLevel> Levels { get; set; } = [];
}