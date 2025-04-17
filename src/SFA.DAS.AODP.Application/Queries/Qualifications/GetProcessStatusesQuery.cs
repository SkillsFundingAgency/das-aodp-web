using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications;

public class GetProcessStatusesQuery : IRequest<BaseMediatrResponse<GetProcessStatusesQueryResponse>>
{
}
