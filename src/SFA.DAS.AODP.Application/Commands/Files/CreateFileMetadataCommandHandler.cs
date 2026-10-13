using MediatR;
using SFA.DAS.AODP.Domain.Files;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Files
{
    public class CreateFileMetadataCommandHandler : IRequestHandler<CreateFileMetadataCommand, BaseMediatrResponse<EmptyResponse>>
    {
        private readonly IApiClient _apiClient;

        public CreateFileMetadataCommandHandler(IApiClient apiCLient)
        {
            _apiClient = apiCLient;
        }

        public async Task<BaseMediatrResponse<EmptyResponse>> Handle(CreateFileMetadataCommand command, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<EmptyResponse>();

            try
            {
                await _apiClient.PostWithResponseCode<EmptyResponse>(new CreateFileMetadataApiRequest { Data = command });
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
