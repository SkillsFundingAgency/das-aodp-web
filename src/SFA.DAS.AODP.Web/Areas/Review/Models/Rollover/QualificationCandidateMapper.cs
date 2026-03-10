namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public static class QualificationCandidateMapper
    {
        public static QualificationCandidate Map(IReadOnlyDictionary<string, string> row)
        {
            string Get(string key) => row.TryGetValue(key, out var v) ? v : string.Empty;

            return new QualificationCandidate
            {
                QualificationNumber = Get(QualificationImportColumns.QualificationNumber),
                Title = Get(QualificationImportColumns.Title),
                AwardingOrganisation = Get(QualificationImportColumns.AwardingOrganisation),
                FundingOffer = Get(QualificationImportColumns.FundingOffer),
                FundingApprovalEndDate = Get(QualificationImportColumns.FundingApprovalEndDate)
            };
        }
    }
}