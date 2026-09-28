namespace SFA.DAS.AODP.Domain.Rollover;

public record RolloverImportStatus
{
    public DateTime? RegulatedQualificationsLastImported { get; set; }
    public DateTime? FundedQualificationsLastImported { get; set; }
    public DateTime? DefundingListLastImported { get; set; }
    public DateTime? PldnsListLastImported { get; set; }

    public Rollover SetImportStatus(
        Rollover session, 
        DateTime? regulatedQualificationsLastImported,
        DateTime? fundedQualificationsLastImported, 
        DateTime? defundingListLastImported,
        DateTime? pldnsListLastImported)
    {
        RegulatedQualificationsLastImported = regulatedQualificationsLastImported;
        FundedQualificationsLastImported = fundedQualificationsLastImported;
        DefundingListLastImported = defundingListLastImported;
        PldnsListLastImported = pldnsListLastImported;

        session.ImportStatus = this;

        return session;
    }
}