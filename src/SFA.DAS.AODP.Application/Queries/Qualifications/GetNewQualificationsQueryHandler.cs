using MediatR;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Test
{
    public class GetNewQualificationsQueryHandler : IRequestHandler<GetNewQualificationsQuery, BaseMediatrResponse<GetNewQualificationsQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetNewQualificationsQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetNewQualificationsQueryResponse>> Handle(GetNewQualificationsQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetNewQualificationsQueryResponse>();
            response.Success = false;
            try
            {
                var result = await _apiClient.Get<BaseMediatrResponse<GetNewQualificationsQueryResponse>>(new GetNewQualificationsApiRequest());
                if (result != null && result.Value != null)
                {
                    response.Value = result.Value;
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.ErrorMessage = "No new qualifications found.";
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }

            return response;
        }
    }
}

