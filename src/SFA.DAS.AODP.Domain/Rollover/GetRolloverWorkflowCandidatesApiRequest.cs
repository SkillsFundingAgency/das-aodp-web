using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Rollover;

public class GetRolloverCandidatesApiRequest : IGetApiRequest
{
    public string GetUrl => "api/rollover/rollovercandidates";
}