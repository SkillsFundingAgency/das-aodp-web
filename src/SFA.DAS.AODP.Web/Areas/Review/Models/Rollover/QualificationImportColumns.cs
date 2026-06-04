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
            QualificationName,
            AwardingOrganisation,
            QualificationNumber,
            Level,
            QualificationType,
            Subcategory,
            SectorSubjectArea,
            Status
        };

        public static string ColumnNamesForView()
        {
            return Required.Count switch
            {
                0 => string.Empty,
                1 => Required[0],
                _ => string.Join(", ", Required.Take(Required.Count - 1))
                     + " and "
                     + Required[^1]
            };
        }
    }
}