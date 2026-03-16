using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Review
{
    public class BulkSaveReviewerCommandHandler
        : IRequestHandler<BulkSaveReviewerCommand, BaseMediatrResponse<BulkSaveReviewerCommandResponse>>
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<BulkSaveReviewerCommandHandler> _logger;
        public BulkSaveReviewerCommandHandler(IApiClient apiClient, ILogger<BulkSaveReviewerCommandHandler> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<BaseMediatrResponse<BulkSaveReviewerCommandResponse>> Handle(
            BulkSaveReviewerCommand request,
            CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<BulkSaveReviewerCommandResponse>()
            {
                Success = false
            };

            try
            {
                _logger.LogInformation(
                    "WEB HANDLER: Sending BulkSaveReviewer to outer API. Count={Count}, Reviewer1={Reviewer1}, Reviewer2={Reviewer2}, PutUrl={PutUrl}",
                    request?.ApplicationReviewIds?.Count,
                    request?.Reviewer1,
                    request?.Reviewer2,
                    new BulkSaveReviewerApiRequest().PutUrl);

                var result = await _apiClient.PutWithResponseCode<BulkSaveReviewerCommandResponse>(
                    new BulkSaveReviewerApiRequest()
                    {
                        Data = request
                    });

                response.Value = result.Body;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.Success = false;
            }

            return response;
        }
    }
}
