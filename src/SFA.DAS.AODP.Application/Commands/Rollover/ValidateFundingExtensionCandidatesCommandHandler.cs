using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Commands.Rollover
{
    public class ValidateFundingExtensionCandidatesCommandHandler
        : IRequestHandler<ValidateFundingExtensionCandidatesCommand, BaseMediatrResponse<ValidateFundingExtensionCandidatesCommandResponse>>
    {
        private readonly IApiClient _apiClient;

        public ValidateFundingExtensionCandidatesCommandHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;

        }

        public async Task<BaseMediatrResponse<ValidateFundingExtensionCandidatesCommandResponse>> Handle(ValidateFundingExtensionCandidatesCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<ValidateFundingExtensionCandidatesCommandResponse>();

            try
            {
                var result = await _apiClient.PostWithResponseCode<ValidateFundingExtensionCandidatesCommandResponse>(new ValidateFundingExtensionCandidatesApiRequest()
                {
                    Data = request
                });

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
