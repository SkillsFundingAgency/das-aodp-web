using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Routes;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Routes
{
    public class DeleteRouteCommandHandler : IRequestHandler<DeleteRouteCommand, BaseMediatrResponse<EmptyResponse>>
    {
        private readonly IApiClient _apiClient;


        public DeleteRouteCommandHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;

        }

        public async Task<BaseMediatrResponse<EmptyResponse>> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<EmptyResponse>()
            {
                Success = false
            };

            try
            {
                var apiRequest = new DeleteRouteApiRequest()
                {
                    FormVersionId = request.FormVersionId,
                    SectionId = request.SectionId,
                    PageId = request.PageId,
                    QuestionId = request.QuestionId,
                };
                await _apiClient.Delete(apiRequest);
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
