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
                var result = await _apiClient.Get<BaseMediatrResponse<GetQualificationOutputFileResponse>>(
                    new GetQualificationOutputFileApiRequest());

                if (result is null || !result.Success || result.Value is null ||
                    result.Value.ZipFileContent is null || result.Value.ZipFileContent.Length == 0)
                {
                    response.Success = false;
                    response.ErrorMessage = result?.ErrorMessage ?? "Output file not available.";

                }
                else
                {
                    response.Success = true;
                    response.Value = result.Value;
                }
                
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
            return response;
        }
    }
}
