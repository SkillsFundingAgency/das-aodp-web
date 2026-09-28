using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Rollover;

public class GetQualificationVersionsForRolloverQueryBuilderApiRequest(RolloverQueryBuilderRequest data) : IPostMultipartJsonFileApiRequest
{
    public string PostUrl => "api/rollover/querybuilder/qualificationversions";

    public object Data { get; } = data;
}
