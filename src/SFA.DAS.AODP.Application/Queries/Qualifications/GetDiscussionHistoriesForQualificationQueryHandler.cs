using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications;

public class GetDiscussionHistoriesForQualificationQueryHandler(IApiClient apiClient) : IRequestHandler<GetDiscussionHistoriesForQualificationQuery, BaseMediatrResponse<QualificationDiscussionHistoriesResponse>>
{
    private readonly IApiClient _apiClient = apiClient;

    public async Task<BaseMediatrResponse<QualificationDiscussionHistoriesResponse>> Handle(GetDiscussionHistoriesForQualificationQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<QualificationDiscussionHistoriesResponse>();
        try
        {
            var result = await _apiClient.Get<QualificationDiscussionHistoriesResponse>(new GetDiscussionHistoryForQualificationApiRequest(request.QualificationReference));
            if (result != null)
            {
                response.Value = result;
                response.Success = true;
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"Failed to get qualification discussion history for qualification ref: {request.QualificationReference}";
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }
        return response;
    }
}
