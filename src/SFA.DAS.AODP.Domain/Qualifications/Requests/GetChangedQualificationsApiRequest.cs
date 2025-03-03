using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class GetChangedQualificationsApiRequest : IGetApiRequest
    {
        public string GetUrl => "api/qualifications?status=changed";
    }
}