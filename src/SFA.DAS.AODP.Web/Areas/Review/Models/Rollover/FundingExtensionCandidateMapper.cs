namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public static class FundingExtensionCandidateMapper
    {
        public static FundingExtensionCandidate Map(IReadOnlyDictionary<string, string> row)
        {
            string Get(string key) => row.TryGetValue(key, out var v) ? v : string.Empty;

            DateTime? ParseDate(string value)
            {
                if (DateTime.TryParse(value, out var date))
                    return date;

                return null;
            }

            bool? ParseBool(string value)
            {
                if (bool.TryParse(value, out var b))
                    return b;

                return null;
            }

            return new FundingExtensionCandidate
            {
                Qan = Get(FundingExtensionCandidateColumns.Qan),
                QualificationTitle = Get(FundingExtensionCandidateColumns.QualificationTitle),
                QualificationLevel = Get(FundingExtensionCandidateColumns.QualificationLevel),
                QualificationType = Get(FundingExtensionCandidateColumns.QualificationType),
                Ssa = Get(FundingExtensionCandidateColumns.Ssa),
                OperationalEndDate = ParseDate(Get(FundingExtensionCandidateColumns.OperationalEndDate)),
                OfferedInEngland = ParseBool(Get(FundingExtensionCandidateColumns.OfferedInEngland)),
                Glh = Get(FundingExtensionCandidateColumns.Glh),
                Tqt = Get(FundingExtensionCandidateColumns.Tqt),
                PreSixteen = ParseBool(Get(FundingExtensionCandidateColumns.PreSixteen)),
                SixteenToEighteen = ParseBool(Get(FundingExtensionCandidateColumns.SixteenToEighteen)),
                EighteenPlus = ParseBool(Get(FundingExtensionCandidateColumns.EighteenPlus)),
                NineteenPlus = ParseBool(Get(FundingExtensionCandidateColumns.NineteenPlus)),
                FundingStreamName = Get(FundingExtensionCandidateColumns.FundingStreamName),
                FundingApprovalStartDate = ParseDate(Get(FundingExtensionCandidateColumns.FundingApprovalStartDate)),
                RollOverStatus = Get(FundingExtensionCandidateColumns.RollOverStatus),
                ExclusionReason = Get(FundingExtensionCandidateColumns.ExclusionReason),
                CurrentFundingApprovalEndDate = ParseDate(Get(FundingExtensionCandidateColumns.CurrentFundingApprovalEndDate)),
                ProposedFundingApprovalEndDate = ParseDate(Get(FundingExtensionCandidateColumns.ProposedFundingApprovalEndDate))
            };
        }
    }
}