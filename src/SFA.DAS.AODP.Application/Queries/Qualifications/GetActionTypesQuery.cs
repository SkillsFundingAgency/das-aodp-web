using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetActionTypesQuery: IRequest<BaseMediatrResponse<GetActionTypesResponse>>
    {
    }
}
