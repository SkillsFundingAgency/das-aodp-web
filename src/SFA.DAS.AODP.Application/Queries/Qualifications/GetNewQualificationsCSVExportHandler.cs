using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetNewQualificationsCsvExportHandler : IRequestHandler<GetNewQualificationsCsvExportQuery, BaseMediatrResponse<GetQualificationsExportResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetNewQualificationsCsvExportHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetQualificationsExportResponse>> Handle(GetNewQualificationsCsvExportQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetQualificationsExportResponse>();

            try
            {
                var result = await _apiClient.Get<GetQualificationsExportResponse>(new GetNewQualificationCsvExportApiRequest());
                if (result?.QualificationExports == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "No New qualifications found.";
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
