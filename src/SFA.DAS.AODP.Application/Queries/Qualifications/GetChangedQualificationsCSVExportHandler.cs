using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetChangedQualificationsCsvExportHandler : IRequestHandler<GetChangedQualificationsCsvExportQuery, BaseMediatrResponse<GetQualificationsExportResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetChangedQualificationsCsvExportHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetQualificationsExportResponse>> Handle(GetChangedQualificationsCsvExportQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetQualificationsExportResponse>();

            try
            {
                var result = await _apiClient.Get<GetQualificationsExportResponse>(new GetChangedQualificationCsvExportApiRequest());
                if (result?.QualificationExports == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "No Changed qualifications found.";
                }
                else
                {
                    response.Value = new GetQualificationsExportResponse
                    {
                        QualificationExports = result.QualificationExports
                    };
                    response.Success = true;
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
}
