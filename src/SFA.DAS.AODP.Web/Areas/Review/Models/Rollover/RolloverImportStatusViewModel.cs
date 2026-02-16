namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public class RolloverImportStatusViewModel
{
    public DateTime? RegulatedQualificationsLastImported { get; set; }
    public DateTime? FundedQualificationsLastImported { get; set; }
    public DateTime? DefundingListLastImported { get; set; }
    public DateTime? PldnsListLastImported { get; set; }

    public static RolloverImportStatusViewModel MapFromSession(RolloverImportStatusSession? session)
    {
        if (session is null)
        {
            return new RolloverImportStatusViewModel();
        }

        return new RolloverImportStatusViewModel
        {
            RegulatedQualificationsLastImported = session.RegulatedQualificationsLastImported,
            FundedQualificationsLastImported = session.FundedQualificationsLastImported,
            DefundingListLastImported = session.DefundingListLastImported,
            PldnsListLastImported = session.PldnsListLastImported
        };
    }

    public static RolloverImportStatusSession MapToSession(RolloverImportStatusViewModel? model)
    {
        if (model is null)
        {
            return new RolloverImportStatusSession();
        }

        return new RolloverImportStatusSession
        {
            RegulatedQualificationsLastImported = model.RegulatedQualificationsLastImported,
            FundedQualificationsLastImported = model.FundedQualificationsLastImported,
            DefundingListLastImported = model.DefundingListLastImported,
            PldnsListLastImported = model.PldnsListLastImported
        };
    }
}
