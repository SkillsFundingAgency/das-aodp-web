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

            var result = await _apiClient.Get<GetQualificationsQueryResponse>(apiRequest);

            if (result?.Qualifications != null)
            {
                response.Value.Qualifications = result.Qualifications;
                response.Value.TotalRecords = result.TotalRecords;
                response.Value.Skip = result.Skip;
                response.Value.Take = result.Take;
                response.Success = true;
                return response;
            }

            response.ErrorMessage = "No matching qualifications found.";
            response.Value.Qualifications = new List<GetMatchingQualificationsQueryItem>();
            return response;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
