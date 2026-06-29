using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Rollover;

public class GetQualificationVersionsForRolloverQueryBuilderApiRequest(RolloverQueryBuilderRequest data) : IPostApiRequest
{
    public object Data { get; set; } = data;

    public string PostUrl => "api/rollover/querybuilder/qualificationversions";
}
