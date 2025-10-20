using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationOutputFileLogQueryHandler : IRequestHandler<GetQualificationOutputFileLogQuery, BaseMediatrResponse<GetQualificationOutputFileLogResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetQualificationOutputFileLogQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetQualificationOutputFileLogResponse>> Handle(GetQualificationOutputFileLogQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetQualificationOutputFileLogResponse>();

            try
            {
                var result = await _apiClient.Get<GetQualificationOutputFileLogResponse>(new GetQualificationOutputFileLogApiRequest());

                if (result is null || result.OutputFileLogs is null || !result.OutputFileLogs.Any())
                {
                    response.Success = false;
                    response.ErrorMessage = "No output file logs available.";
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
