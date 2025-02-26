using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetNewQualificationsCsvExportHandler : IRequestHandler<GetNewQualificationsCsvExportQuery, BaseMediatrResponse<GetNewQualificationsCsvExportResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetNewQualificationsCsvExportHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetNewQualificationsCsvExportResponse>> Handle(GetNewQualificationsCsvExportQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetNewQualificationsCsvExportResponse>();

            try
            {
                var result = await _apiClient.Get<BaseMediatrResponse<GetNewQualificationsCsvExportResponse>>(new GetNewQualificationCsvExportApiRequest());
                if (result == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "No new qualifications found.";
                }
                else if (result.Value.QualificationExports.Any())
                {
                    response.Value = new GetNewQualificationsCsvExportResponse
                    {
                        QualificationExports = result.Value.QualificationExports
                    };
                    response.Success = true;
                }
                else
                {
                    response.Value = new GetNewQualificationsCsvExportResponse
                    {
                        QualificationExports = new List<QualificationExport>()
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
