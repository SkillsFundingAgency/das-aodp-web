namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public static class QualificationImportColumns
    {
        public const string QualificationNumber = "qualification number";
        public const string Title = "title";
        public const string AwardingOrganisation = "awarding organisation";
        public const string FundingOffer = "funding offer";
        public const string FundingApprovalEndDate = "funding approval end date";

        public static readonly IReadOnlyList<string> Required = new[]
        {
            QualificationNumber,
            Title,
            AwardingOrganisation,
            FundingOffer,
            FundingApprovalEndDate
        };
    }
}