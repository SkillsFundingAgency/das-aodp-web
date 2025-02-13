using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class GetNewQualificationsApiRequest : IGetApiRequest
    {
        public string GetUrl => "api/new-qualifications";
    }

}
