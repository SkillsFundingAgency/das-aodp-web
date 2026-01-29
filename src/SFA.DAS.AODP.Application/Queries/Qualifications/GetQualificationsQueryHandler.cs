using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications;

public class GetQualificationsQueryHandler : IRequestHandler<GetQualificationsQuery, BaseMediatrResponse<GetQualificationsQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetQualificationsQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetQualificationsQueryResponse>> Handle(GetQualificationsQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetQualificationsQueryResponse>();
        response.Success = false;

        try
        {
            var apiRequest = new GetQualificationsApiRequest(
                request.SearchTerm!,
                request.Skip,
                request.Take
            );


            {
                }
            };

            await Task.CompletedTask;

        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
