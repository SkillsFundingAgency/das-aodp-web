using MediatR;
namespace SFA.DAS.AODP.Application.Queries.Users;
public class GetUsersQuery : IRequest<BaseMediatrResponse<GetUsersQueryResponse>>
{
    public GetUsersQuery()
    {
    }
}
