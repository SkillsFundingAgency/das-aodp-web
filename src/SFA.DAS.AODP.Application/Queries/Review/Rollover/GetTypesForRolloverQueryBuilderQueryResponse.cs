namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public record GetTypesForRolloverQueryBuilderQueryResponse
{
    public IEnumerable<QualificationType> Types { get; set; } = [];
}