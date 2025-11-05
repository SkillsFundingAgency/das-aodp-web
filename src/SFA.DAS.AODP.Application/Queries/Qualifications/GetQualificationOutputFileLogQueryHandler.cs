using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationOutputFileLogQueryHandler : IRequestHandler<GetQualificationOutputFileLogQuery, BaseMediatrResponse<GetQualificationOutputFileLogResponse>>
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<GetQualificationOutputFileLogQueryHandler> _logger;

        public GetQualificationOutputFileLogQueryHandler(IApiClient apiClient, ILogger<GetQualificationOutputFileLogQueryHandler> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<BaseMediatrResponse<GetQualificationOutputFileLogResponse>> Handle(GetQualificationOutputFileLogQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetQualificationOutputFileLogResponse>();

            try
            {
                var result = await _apiClient.Get<BaseMediatrResponse<GetQualificationOutputFileLogResponse>>(
                    new GetQualificationOutputFileLogApiRequest());

                if (result is null || !result.Success || !result.Value.OutputFileLogs.Any())
                {
                    response.Success = false;
                    response.ErrorMessage = "No output file logs available.";
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
                response.ErrorMessage = "An unexpected issue occurred while retrieving output file logs.";
                _logger.LogError(ex, response.ErrorMessage);
            }

            return response;
        }
    }
}
