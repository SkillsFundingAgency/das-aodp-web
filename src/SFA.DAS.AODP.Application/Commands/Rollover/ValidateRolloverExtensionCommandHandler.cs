using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Commands.Rollover
{
    public class ValidateRolloverExtensionCommandHandler
        : IRequestHandler<ValidateRolloverExtensionCommand, BaseMediatrResponse<ValidateRolloverExtensionCommandResponse>>
    {
        private readonly IApiClient _apiClient;

        public ValidateRolloverExtensionCommandHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;

        }

        public async Task<BaseMediatrResponse<ValidateRolloverExtensionCommandResponse>> Handle(ValidateRolloverExtensionCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<ValidateRolloverExtensionCommandResponse>();

            try
            {
                var result = await _apiClient.PostWithResponseCodeAsJsonFile<ValidateRolloverExtensionCommandResponse>(new ValidateRolloverExtensionApiRequest()
                {
                    Data = request
                });

                if (result == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "The API returned no data.";
                    return response;
                }

                response.Value = result;
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
