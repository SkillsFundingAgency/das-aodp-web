using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Qualifications
{
    public class GetNewQualificationsApiRequest : IGetApiRequest
    {
        public string GetUrl => "api/new-qualifications";
    }

}
