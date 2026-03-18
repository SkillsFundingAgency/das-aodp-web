using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover
{
    public class RolloverEligibilityDates
    {
        public RolloverEligibilityDate? FundingEndDate { get; set; }
        public RolloverEligibilityDate? OperationalEndDate { get; set; }
    }
}