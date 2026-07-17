using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications;

public class GetDiscussionHistoriesForQualificationQuery : IRequest<BaseMediatrResponse<QualificationDiscussionHistoriesResponse>>
{
    public string QualificationReference { get; set; } = string.Empty;
}
