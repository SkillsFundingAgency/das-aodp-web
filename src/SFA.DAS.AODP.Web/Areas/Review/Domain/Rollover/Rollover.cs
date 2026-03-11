using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

public class Rollover
{
    public RolloverStart? Start { get; set; }
    public RolloverImportStatus? ImportStatus { get; set; }
    public RolloverPreviousData? PreviousData { get; set; }
    public List<QualificationCandidate> RolloverCandidates { get; set; } = new();
    public RolloverFundingStream? RolloverFundingStream { get; set; }
}

public class RolloverFundingStream
{
    public List<FundingStream> FundingStreams { get; set; } = new();
    public List<string> SelectedIds { get; set; } = new();
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