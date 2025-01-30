using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Routes;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Routes
{
    public class ConfigureRoutingForQuestionCommandHandler : IRequestHandler<ConfigureRoutingForQuestionCommand, BaseMediatrResponse<ConfigureRoutingForQuestionCommandResponse>>
    {
        private readonly IApiClient _apiClient;


        public ConfigureRoutingForQuestionCommandHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;

        }

        public async Task<BaseMediatrResponse<ConfigureRoutingForQuestionCommandResponse>> Handle(ConfigureRoutingForQuestionCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<ConfigureRoutingForQuestionCommandResponse>()
            {
                Success = false
            };

            try
            {
                var apiRequest = new ConfigureRoutingForQuestionApiRequest(request.QuestionId, request.PageId, request.FormVersionId, request.SectionId)
                {
                    Data = request
                };
                await _apiClient.Put(apiRequest);
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
