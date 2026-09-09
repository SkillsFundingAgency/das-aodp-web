using MediatR;
using SFA.DAS.AODP.Domain.Files;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Files
{
    public class DeleteFileMetadataCommandHandler : IRequestHandler<DeleteFileMetataCommand, BaseMediatrResponse<EmptyResponse>>
    {
        private readonly IApiClient _apiClient;

        public DeleteFileMetadataCommandHandler(IApiClient apiCLient)
        {
            _apiClient = apiCLient;
        }

        public async Task<BaseMediatrResponse<EmptyResponse>> Handle(DeleteFileMetataCommand command, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<EmptyResponse>();

            try
            {
                await _apiClient.PostWithResponseCode<EmptyResponse>(new DeleteFileMetadataApiRequest { Data = command });
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
