using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Commands.Rollover
{
    public class SubmitRolloverExtensionCommandHandler
        : IRequestHandler<SubmitRolloverExtensionCommand, BaseMediatrResponse<SubmitRolloverExtensionCommandResponse>>
    {
        private readonly IApiClient _apiClient;
        public SubmitRolloverExtensionCommandHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<SubmitRolloverExtensionCommandResponse>> Handle(
            SubmitRolloverExtensionCommand request, 
            CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<SubmitRolloverExtensionCommandResponse>();

            try
            {
                var result = await _apiClient.PostWithResponseCodeAsJsonFile<SubmitRolloverExtensionCommandResponse>(new SubmitRolloverExtensionApiRequest()
                {
                    Data = request
                });

                if (result == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "The API returned no data.";
                    return response;
                }

                response.Value =  result;
                response.Success = true;
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
