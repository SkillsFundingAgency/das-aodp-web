using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests
{
    public class GetNewQualificationCSVExportApiRequest : IGetApiRequest
    {
        public string GetUrl => "/api/qualifications/export?status=new";
    }
}
