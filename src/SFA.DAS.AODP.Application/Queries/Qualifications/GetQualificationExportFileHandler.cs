using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationsExportFileHandler : IRequestHandler<GetQualificationExportFileQuery, BaseMediatrResponse<GetQualificationExportFileResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetQualificationsExportFileHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetQualificationExportFileResponse>> Handle(GetQualificationExportFileQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetQualificationExportFileResponse>();

            try
            {
                var result = await _apiClient.Get<GetQualificationExportFileResponse>(new GetQualificationExportFileApiRequest());
                if(result is null || result.ZipFileContent is null || result.ZipFileContent.Length == 0)
                {
                    response.Success = false;
                    response.ErrorMessage = "Export file not available.";
                }
                else
                {
                    response.Success = true;
                    response.Value = result; 
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
