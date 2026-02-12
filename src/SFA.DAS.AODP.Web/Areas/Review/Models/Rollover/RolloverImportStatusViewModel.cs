namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public class RolloverImportStatusViewModel
{
    public DateTime? RegulatedQualificationsLastImported { get; set; }
    public DateTime? FundedQualificationsLastImported { get; set; }
    public DateTime? DefundingListLastImported { get; set; }
    public DateTime? PldnsListLastImported { get; set; }
}
