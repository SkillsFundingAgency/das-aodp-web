using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class BulkUpdateQualificationStatusApiRequest : IPutApiRequest
    {
        public string PutUrl => $"api/qualifications/bulk-status";
        public object Data { get; set; }
    }
}
