using SFA.DAS.AODP.Models.Application;

namespace SFA.DAS.AODP.Web.Extensions
{
    public static class ApplicationStatusExtensions
    {
        public static bool IsWithdrawable(this ApplicationStatus status)
            => status is ApplicationStatus.InReview
            or ApplicationStatus.Reviewed
            or ApplicationStatus.OnHold;
    }
}
