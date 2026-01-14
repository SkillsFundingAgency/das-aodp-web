using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class GetQualificationOutputFileApiRequest : IPostApiRequest
    {
        public string PostUrl => $"api/qualifications/outputfile";
        public object Data { get; set; }
    }
}
