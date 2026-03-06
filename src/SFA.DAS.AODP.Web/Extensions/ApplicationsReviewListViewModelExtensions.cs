using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;
using SFA.DAS.AODP.Web.Models.Applications;

namespace SFA.DAS.AODP.Web.Extensions
{
    public static class ApplicationsReviewListViewModelExtensions
    {
        public static ApplicationsReviewQuery ToApplicationQuery(
            this ApplicationsReviewListViewModel model)
        {
            return new ApplicationsReviewQuery
            {
                Status = model.Status,
                PageNumber = model.PageNumber,
                RecordsPerPage = model.RecordsPerPage,
                ApplicationSearch = model.ApplicationSearch,
                AwardingOrganisationSearch = model.AwardingOrganisationSearch,
                //ReviewerSelection = model.ReviewerSelection
            };
        }
    }
}
