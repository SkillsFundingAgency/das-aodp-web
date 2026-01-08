using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications;

public class GetQualificationsQuery : IRequest<BaseMediatrResponse<GetQualificationsQueryResponse>>
{
    public string? Name { get; set; }
    public string? Organisation { get; set; }
    public string? QAN { get; set; }
    public string? Status { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}