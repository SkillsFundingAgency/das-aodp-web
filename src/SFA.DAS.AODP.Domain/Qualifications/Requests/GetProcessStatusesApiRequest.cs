using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Qualifications.Requests;


public class GetProcessStatusesApiRequest : IGetApiRequest
{
    public string GetUrl => "api/qualifications/processstatuses";
}
