namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class RolloverSummaryViewModel
    {
        public int RolloverCandidatesInRun { get; set; } = 12;

        public int AvailableRolloverCandidates { get; set; } = 23;

        public int EligibleCandidatesInRun { get; set; } = 10;

        public int IneligibleCandidatesInRun { get; set; } = 2;

        public int RemainingRolloverCandidates { get; set; } = 11;
    }
}
