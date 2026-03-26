using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Web.Constants;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.Applications
{
    [ExcludeFromCodeCoverage]
    public class ApplicationsReviewQuery
    {
        public int PageNumber { get; set; } = 1;
        public int RecordsPerPage { get; set; } = 10;
        public string? ApplicationSearch { get; set; } = string.Empty;
        public string? AwardingOrganisationSearch { get; set; } = string.Empty;
        public string? ReviewerSelection { get; set; } = string.Empty;
        public List<ApplicationStatus>? Status { get; set; }

        public object ToRouteValues(int? pageNumberOverride = null) => new
        {
            pageNumber = pageNumberOverride ?? PageNumber,
            recordsPerPage = RecordsPerPage,
            applicationSearch = ApplicationSearch,
            awardingOrganisationSearch = AwardingOrganisationSearch,
            reviewerSelection = ReviewerSelection,
            status = Status
        };

        public GetApplicationsForReviewQuery ToGetApplicationsForReviewQuery(string reviewUser)
        {
            var reviewerSelection = ReviewerSelection?.Trim();

            var unassignedOnly = string.Equals(
                reviewerSelection,
                ReviewerDropdown.UnassignedValue,
                StringComparison.Ordinal);

            var reviewerSearch =
                unassignedOnly || string.IsNullOrWhiteSpace(reviewerSelection)
                    ? null
                    : reviewerSelection;

            return new GetApplicationsForReviewQuery
            {
                ApplicationSearch = string.IsNullOrWhiteSpace(ApplicationSearch) ? null : ApplicationSearch.Trim(),
                AwardingOrganisationSearch = string.IsNullOrWhiteSpace(AwardingOrganisationSearch) ? null : AwardingOrganisationSearch.Trim(),
                ReviewerSearch = reviewerSearch,
                UnassignedOnly = unassignedOnly,
                ApplicationStatuses = Status?.Select(s => s.ToString()).ToList() ?? new List<string>(),
                ReviewUser = reviewUser,
                Limit = RecordsPerPage,
                Offset = (PageNumber - 1) * RecordsPerPage
            };
        }

    }
}
