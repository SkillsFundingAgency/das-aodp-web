using SFA.DAS.AODP.Models.Application;

namespace SFA.DAS.AODP.Web.Models.Session
{
    public class ApplicationsReviewFilterSessionModel
    {
        public int PageNumber { get; set; } = 1;
        public int RecordsPerPage { get; set; } = 10;

        public string? ApplicationSearch { get; set; } = string.Empty;
        public string? AwardingOrganisationSearch { get; set; } = string.Empty;
        public string? ReviewerSelection { get; set; } = string.Empty;

        public List<ApplicationStatus>? Status { get; set; }
    }

}
