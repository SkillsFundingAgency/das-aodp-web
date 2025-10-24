using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class GetQualificationOutputFileApiRequest : IGetApiRequest
    {
        public string GetUrl => "api/qualifications/outputfile";
    }
}
