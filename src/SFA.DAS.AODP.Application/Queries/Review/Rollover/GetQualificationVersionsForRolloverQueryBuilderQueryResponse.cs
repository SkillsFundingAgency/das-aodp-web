namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetQualificationVersionsForRolloverQueryBuilderQueryResponse
{
    public IEnumerable<RolloverQueryBuilderCandidatesDto> QualificationVersions { get; set; } = [];
}

public record RolloverQueryBuilderCandidatesDto
{
    public Guid Id { get; set; }
    public Guid QualificationVersionId { get; set; }
    public string? QualificationNumber { get; init; }
    public string? QualificationName { get; init; }
    public Guid FundingOfferId { get; set; }
    public string? FundingOfferName { get; init; }
    public string? AcademicYear { get; set; }
    public int? RolloverRound { get; set; }
    public DateTime? PreviousFundingEndDate { get; set; }
    public DateTime? NewFundingEndDate { get; set; }
}