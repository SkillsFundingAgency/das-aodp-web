using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetChangedQualificationsCsvExportHandler : IRequestHandler<GetChangedQualificationsCsvExportQuery, BaseMediatrResponse<GetChangedQualificationsCsvExportResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetChangedQualificationsCsvExportHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetChangedQualificationsCsvExportResponse>> Handle(GetChangedQualificationsCsvExportQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetChangedQualificationsCsvExportResponse>();

            try
            {
                var result = await _apiClient.Get<BaseMediatrResponse<GetChangedQualificationsCsvExportResponse>>(new GetChangedQualificationCsvExportApiRequest());
                if (result == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "No new qualifications found.";
                }
                else if (result.Value.QualificationExports.Any())
                {
                    response.Value = new GetChangedQualificationsCsvExportResponse
                    {
                        QualificationExports = result.Value.QualificationExports
                    };
                    response.Success = true;
                }
                else
                {
                    response.Value = new GetChangedQualificationsCsvExportResponse
                    {
                        QualificationExports = new List<ChangedExport>()
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
