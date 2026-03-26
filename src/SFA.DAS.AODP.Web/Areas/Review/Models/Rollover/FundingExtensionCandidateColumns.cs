namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public static class FundingExtensionCandidateColumns
    {
        public const string Qan = "qan";
        public const string QualificationTitle = "qualification title";
        public const string QualificationLevel = "qualification level";
        public const string QualificationType = "qualification type";
        public const string Ssa = "ssa";
        public const string OperationalEndDate = "operational end date";
        public const string OfferedInEngland = "offered in england";
        public const string Glh = "glh";
        public const string Tqt = "tqt";
        public const string PreSixteen = "pre sixteen";
        public const string SixteenToEighteen = "sixteen to eighteen";
        public const string EighteenPlus = "eighteen plus";
        public const string NineteenPlus = "nineteen plus";
        public const string FundingStreamName = "funding stream name";
        public const string FundingApprovalStartDate = "funding approval start date";
        public const string RollOverStatus = "rollover status";
        public const string ExclusionReason = "exclusion reason";
        public const string CurrentFundingApprovalEndDate = "current funding approval end date";
        public const string ProposedFundingApprovalEndDate = "proposed funding approval end date";

        public static readonly IReadOnlyList<string> Required = new[]
        {
            Qan,
            QualificationTitle,
            QualificationLevel,
            QualificationType,
            Ssa,
            OperationalEndDate,
            OfferedInEngland,
            Glh,
            Tqt,
            PreSixteen,
            SixteenToEighteen,
            EighteenPlus,
            NineteenPlus,
            FundingStreamName,
            FundingApprovalStartDate,
            RollOverStatus,
            ExclusionReason,
            CurrentFundingApprovalEndDate,
            ProposedFundingApprovalEndDate
        };
    }
}