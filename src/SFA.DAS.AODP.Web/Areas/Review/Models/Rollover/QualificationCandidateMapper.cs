namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public static class QualificationCandidateMapper
    {
        public static QualificationCandidate Map(IReadOnlyDictionary<string, string> row)
        {
            string Get(string key) =>
                row.TryGetValue(key, out var v) ? v ?? string.Empty : string.Empty;

            return new QualificationCandidate
            {
                QualificationNumber = Get(QualificationImportColumns.QualificationNumber),
                QualificationName = Get(QualificationImportColumns.QualificationName),
                AwardingOrganisation = Get(QualificationImportColumns.AwardingOrganisation)
            };
        }
    }
}
