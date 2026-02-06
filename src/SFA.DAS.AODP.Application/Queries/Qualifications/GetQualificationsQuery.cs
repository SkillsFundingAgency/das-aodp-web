using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications;

public class GetQualificationsQuery : IRequest<BaseMediatrResponse<GetQualificationsQueryResponse>>
{
    public string? SearchTerm { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}