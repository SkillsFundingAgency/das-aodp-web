using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Rollover;

public class GetQualificationVersionsForRolloverQueryBuilderApiRequest(RolloverQueryBuilderRequest data) : IPostMultipartFormDataApiRequest
{
    public string PostUrl => "api/rollover/querybuilder/qualificationversions";

    public RolloverQueryBuilderRequest Data { get; } = data;

    public IEnumerable<KeyValuePair<string, string>> FormData => MultipartFormDataMapper.Map(Data);
}
