using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    [ExcludeFromCodeCoverage]
    public class BulkUpdateQualificationStatusApiRequest : IPutApiRequest
    {
        public string PutUrl => $"api/qualifications/bulk-status";
        public object Data { get; set; }
    }
}
