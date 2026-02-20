using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public class RolloverImportStatusViewModel
{
    public DateTime? RegulatedQualificationsLastImported { get; set; }
    public DateTime? FundedQualificationsLastImported { get; set; }
    public DateTime? DefundingListLastImported { get; set; }
    public DateTime? PldnsListLastImported { get; set; }

    public static RolloverImportStatusViewModel MapFromSession(RolloverImportStatus? session)
    => new RolloverImportStatusViewModel
        {
            RegulatedQualificationsLastImported = session.RegulatedQualificationsLastImported,
            FundedQualificationsLastImported = session.FundedQualificationsLastImported,
            DefundingListLastImported = session.DefundingListLastImported,
            PldnsListLastImported = session.PldnsListLastImported
        };
    

    public static RolloverImportStatus MapToSession(RolloverImportStatusViewModel? model)
    =>  new RolloverImportStatus
        {
            RegulatedQualificationsLastImported = model.RegulatedQualificationsLastImported,
            FundedQualificationsLastImported = model.FundedQualificationsLastImported,
            DefundingListLastImported = model.DefundingListLastImported,
            PldnsListLastImported = model.PldnsListLastImported
        };
    
}
