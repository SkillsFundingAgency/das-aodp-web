using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Commands.Rollover
{
    public class ApplyFundingExtensionsCommandHandler
        : IRequestHandler<ApplyFundingExtensionsCommand, BaseMediatrResponse<ApplyFundingExtensionsCommandResponse>>
    {
        private readonly IApiClient _apiClient;
        public ApplyFundingExtensionsCommandHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<ApplyFundingExtensionsCommandResponse>> Handle(
            ApplyFundingExtensionsCommand request, 
            CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<ApplyFundingExtensionsCommandResponse>();

            try
            {
                var result = await _apiClient.PostWithResponseCode<ApplyFundingExtensionsCommandResponse>(new ApplyFundingExtensionsApiRequest()
                {
                    Data = request
                });

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
