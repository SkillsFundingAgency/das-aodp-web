using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Review
{
    public class BulkSaveReviewerCommandHandler
        : IRequestHandler<BulkSaveReviewerCommand, BaseMediatrResponse<BulkSaveReviewerCommandResponse>>
    {
        private readonly IApiClient _apiClient;
        public BulkSaveReviewerCommandHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
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
