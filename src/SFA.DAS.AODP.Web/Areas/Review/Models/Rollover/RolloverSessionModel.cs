namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public class RolloverSessionModel
{
    public RolloverStartSession? Start { get; set; }

    public RolloverImportStatusSession? ImportStatus { get; set; }
}

public class RolloverStartSession
{
    public RolloverProcess? SelectedProcess { get; set; }
}

public class RolloverImportStatusSession
{
    public DateTime? RegulatedQualificationsLastImported { get; set; }
    public DateTime? FundedQualificationsLastImported { get; set; }
    public DateTime? DefundingListLastImported { get; set; }
    public DateTime? PldnsListLastImported { get; set; }
}