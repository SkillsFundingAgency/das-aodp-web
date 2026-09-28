using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Rollover;

public record GetSectorSubjectAreaForRolloverQueryBuilderApiRequest(RolloverQueryBuilderSectorSubjectAreaRequest data) : IPostApiRequest
{
    public object Data { get; set; } = data;

    public string PostUrl => "api/rollover/querybuilder/sectorsubjectarea";
}