using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Users;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, BaseMediatrResponse<GetUsersQueryResponse>>
{
    private readonly IApiClient _apiClient;
    public GetUsersQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetUsersQueryResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetUsersQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetUsersQueryResponse>(new GetUsersApiRequest());
            response.Value = result;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }

}
