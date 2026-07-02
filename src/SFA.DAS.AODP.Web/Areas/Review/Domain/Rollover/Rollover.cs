using SFA.DAS.AODP.Models.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

[ExcludeFromCodeCoverage]
public record Rollover
{
    public Guid? WorkflowRunId {  get; set; } 
    public RolloverStart? Start { get; set; }
    public RolloverImportStatus? ImportStatus { get; set; }
    public RolloverPreviousData? PreviousData { get; set; }
    public RolloverSelectCandidates? SelectCandidates { get; set; }
    public List<QualificationCandidate> RolloverCandidates { get; set; } = new();
    public List<FundingExtensionCandidate>? RolloverFundingExtensionCandidates { get; set; }
    public RolloverFundingStream? RolloverFundingStream { get; set; }
    public RolloverEligibilityDates? RolloverEligibilityDates { get; set; }
    public RolloverFundingApprovalEndDate? RolloverFundingApprovalEndDate { get; set; }
}