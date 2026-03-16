using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

public record RolloverImportStatus
{
    public DateTime? RegulatedQualificationsLastImported { get; set; }
    public DateTime? FundedQualificationsLastImported { get; set; }
    public DateTime? DefundingListLastImported { get; set; }
    public DateTime? PldnsListLastImported { get; set; }

    public Rollover SetImportStatus(Rollover session, RolloverImportStatusViewModel model)
    {
        session.ImportStatus = RolloverImportStatusViewModel.MapToSession(model);

        return session;
    }
}