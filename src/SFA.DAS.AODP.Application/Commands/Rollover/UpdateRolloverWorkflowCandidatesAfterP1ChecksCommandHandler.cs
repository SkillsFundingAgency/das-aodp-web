using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Commands.Rollover
{
    public class UpdateRolloverWorkflowCandidatesAfterP1ChecksCommandHandler : IRequestHandler<UpdateRolloverWorkflowCandidatesAfterP1ChecksCommand, BaseMediatrResponse<EmptyResponse>>
    {
        private readonly IApiClient _apiClient;

        public UpdateRolloverWorkflowCandidatesAfterP1ChecksCommandHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<EmptyResponse>> Handle(UpdateRolloverWorkflowCandidatesAfterP1ChecksCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<EmptyResponse>();

            try
            {
                await _apiClient.PostWithResponseCode<EmptyResponse>(new UpdateRolloverWorkflowCandidatesAfterP1ChecksApiRequest());
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
