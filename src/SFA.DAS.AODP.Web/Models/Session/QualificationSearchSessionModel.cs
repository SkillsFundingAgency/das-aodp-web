using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Web.Models.Session
{
    public class QualificationSearchSessionModel
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int RecordsPerPage { get; set; } = 10;
    }
}
