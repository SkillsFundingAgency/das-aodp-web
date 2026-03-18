using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Commands.Rollover
{
    public class CreateRolloverWorkflowRunCommandHandler : IRequestHandler<CreateRolloverWorkflowRunCommand, BaseMediatrResponse<CreateRolloverWorkflowRunCommandResponse>>
    {
        private readonly IApiClient _apiClient;

        public CreateRolloverWorkflowRunCommandHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<CreateRolloverWorkflowRunCommandResponse>> Handle(CreateRolloverWorkflowRunCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<CreateRolloverWorkflowRunCommandResponse>();

            try
            {
                var result = await _apiClient.PostWithResponseCode<CreateRolloverWorkflowRunCommandResponse>(new CreateRolloverWorkflowRunApiRequest(request));
                response.Value.RolloverWorkflowRunId = result.RolloverWorkflowRunId;
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
