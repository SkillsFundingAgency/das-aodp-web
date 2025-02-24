using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    class GetNewQualificationsCSVExportHandler : IRequestHandler<GetNewQualificationsCSVExportQuery, BaseMediatrResponse<GetNewQualificationsCSVExportResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetNewQualificationsCSVExportHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetNewQualificationsCSVExportResponse>> Handle(GetNewQualificationsCSVExportQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetNewQualificationsCSVExportResponse>();
            response.Success = false;
            try
            {
                var result = await _apiClient.Get<BaseMediatrResponse<GetNewQualificationsCSVExportResponse>>(new GetNewQualificationCSVExportApiRequest());
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
