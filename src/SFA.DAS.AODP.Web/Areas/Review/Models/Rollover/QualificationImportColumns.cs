namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public static class QualificationImportColumns
    {
        public const string QualificationName = "QualificationName";
        public const string AwardingOrganisation = "AwardingOrganisation";
        public const string QualificationNumber = "QualificationNumber";
        public const string Level = "Level";
        public const string QualificationType = "QualificationType";
        public const string Subcategory = "Subcategory";
        public const string SectorSubjectArea = "SectorSubjectArea";
        public const string Status = "Status";

        public static readonly IReadOnlyList<string> Required = new[]
        {
            QualificationNumber,
            QualificationName,
            AwardingOrganisation,
            Level,
            QualificationType,
            Subcategory,
            SectorSubjectArea,
            Status
        };
    }
}