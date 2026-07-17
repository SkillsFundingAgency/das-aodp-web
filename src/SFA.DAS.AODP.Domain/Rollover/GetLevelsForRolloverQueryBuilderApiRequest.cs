using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Rollover;

public record GetLevelsForRolloverQueryBuilderApiRequest : IGetApiRequest
{
    public string GetUrl => "api/rollover/querybuilder/levels";
}