using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class GetQualificationOutputFileLogApiRequest : IGetApiRequest
    {
        public string GetUrl => $"api/qualifications/outputfile/logs";
    }
}
