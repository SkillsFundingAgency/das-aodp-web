namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public static class FundingExtensionCandidateColumns
    {
        public const string Qan = "QAN";
        public const string QualificationTitle = "QualificationTitle";
        public const string AwardingOrganisation = "AwardingOrganisation";
        public const string QualificationLevel = "QualificationLevel";
        public const string QualificationType = "QualificationType";
        public const string Ssa = "SSA";
        public const string OperationalEndDate = "OperationalEndDate";
        public const string OfferedInEngland = "OfferedInEngland";
        public const string FundedInEngalnd = "FundedInEngland";
        public const string Glh = "GLH";
        public const string Tqt = "TQT";
        public const string Pre16 = "Pre16";
        public const string Age16To18 = "Age16To18";
        public const string Age18Plus = "Age18Plus";
        public const string Age19Plus = "Age19Plus";
        public const string FundingStreamName = "FundingStreamName";
        public const string FundingApprovalStartDate = "FundingApprovalStartDate";
        public const string ProposedOutcome = "ProposedOutcome";
        public const string RollOverStatus = "RollOverStatus";
        public const string ExclusionReason = "ExclusionReason";
        public const string CurrentFundingApprovalEndDate = "CurrentFundingApprovalEndDate";
        public const string ProposedFundingApprovalEndDate = "ProposedFundingApprovalEndDate";
        public const string Comments = "Comments";

        public static readonly IReadOnlyList<string> Required = new[]
        {
            Qan,
            QualificationTitle,
            AwardingOrganisation,
            QualificationLevel,
            QualificationType,
            Ssa,
            OperationalEndDate,
            OfferedInEngland,
            FundedInEngalnd,
            Glh,
            Tqt,
            Pre16,
            Age16To18,
            Age18Plus,
            Age19Plus,
            FundingStreamName,
            FundingApprovalStartDate,
            ProposedOutcome,
            RollOverStatus,
            ExclusionReason,
            CurrentFundingApprovalEndDate,
            ProposedFundingApprovalEndDate,
            Comments
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