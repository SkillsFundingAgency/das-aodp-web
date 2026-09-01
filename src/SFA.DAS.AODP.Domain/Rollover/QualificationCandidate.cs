namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class QualificationCandidate
    {
        public string? QualificationNumber { get; set; }
        public Guid QualificationVersionId { get; set; }
        public string? QualificationName { get; set; }
        public string? AwardingOrganisation { get; set; }
        public Guid FundingOfferId { get; set; }
        public string? FundingOfferName { get; set; }
        public DateTime? FundingApprovalEndDate { get; set; }

        public Guid RolloverCandidateId { get; set; }
        public bool IsActive { get; set; }
        public string? AcademicYear { get; init; }
    }
}