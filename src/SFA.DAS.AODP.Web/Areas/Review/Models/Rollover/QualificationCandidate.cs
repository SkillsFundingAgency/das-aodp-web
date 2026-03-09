namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class QualificationCandidate
    {
        public string? QualificationNumber { get; set; }
        public string? Title { get; set; }
        public string? AwardingOrganisation { get; set; }
        public string? FundingOffer { get; set; }
        public string? FundingApprovalEndDate { get; set; }

        public static QualificationCandidate Map(IReadOnlyDictionary<string, string> row)
        {
            string Get(string key) => row.TryGetValue(key, out var v) ? v : "";

            return new QualificationCandidate
            {
                QualificationNumber = Get("qualification number"),
                Title = Get("title"),
                AwardingOrganisation = Get("awarding organisation"),
                FundingOffer = Get("funding offer"),
                FundingApprovalEndDate = Get("funding approval end date")
            };
        }

        public static readonly string[] Required =
        {
            "qualification number",
            "title",
            "awarding organisation",
            "funding offer",
            "funding approval end date"
        };
    }
}