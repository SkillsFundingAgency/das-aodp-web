using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

public class Rollover
{
    public RolloverStart? Start { get; set; }
    public RolloverImportStatus? ImportStatus { get; set; }
    public RolloverPreviousData? PreviousData { get; set; }
    public RolloverSelectCandidates? SelectCandidates { get; set; }
}

public class RolloverStart
{
    public RolloverProcess? SelectedProcess { get; set; }
}

public class RolloverImportStatus
{
    public DateTime? RegulatedQualificationsLastImported { get; set; }
    public DateTime? FundedQualificationsLastImported { get; set; }
    public DateTime? DefundingListLastImported { get; set; }
    public DateTime? PldnsListLastImported { get; set; }
}

public class RolloverPreviousData
{
    public int CandidateCount { get; set; }
    public RolloverPreviousFileOption? SelectedOption { get; set; }
}

public class RolloverSelectCandidates
{
    public SelectCandidatesForRollover? SelectedOption { get; set; }
    public string? ReturnUrl { get; set; }
}