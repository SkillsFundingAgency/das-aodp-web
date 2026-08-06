using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Web.Models.Session
{
    public class QualificationFilterSessionModel
    {
        public string? QualificationName { get; set; }
        public string? Organisation { get; set; }
        public string? QAN { get; set; }

        public List<Guid> ProcessStatusIds { get; set; } = new();
        public List<AgeGroup> AgeGroups { get; set; } = new();

        public int PageNumber { get; set; } = 1;
        public int RecordsPerPage { get; set; } = 10;
    }
}
