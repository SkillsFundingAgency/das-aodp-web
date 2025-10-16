using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationOutputFileQueryHandler : IRequestHandler<GetQualificationOutputFileQuery, BaseMediatrResponse<GetQualificationOutputFileResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetQualificationOutputFileQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetQualificationOutputFileResponse>> Handle(GetQualificationOutputFileQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetQualificationOutputFileResponse>();

            try
            {
                var result = await _apiClient.Get<GetQualificationOutputFileResponse>(new GetQualificationExportFileApiRequest());
                if(result is null || result.ZipFileContent is null || result.ZipFileContent.Length == 0)
                {
                    response.Success = false;
                    response.ErrorMessage = "Output file not available.";
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
