namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetQualificationVersionsForRolloverQueryBuilderQueryResponse
{
    public IEnumerable<RolloverQualificationVersion> QualificationVersions { get; set; } = [];
}

public class RolloverQualificationVersion
{
    public Guid Id { get; set; }
    public string? QualificationReference { get; set; }
    public string? QualificationName { get; set; }
    public string AwardingOrganisationId { get; set; } = null!;
}
