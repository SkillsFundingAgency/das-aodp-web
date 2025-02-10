using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Routes;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Routes
{
    public class GetAvailableQuestionsForRoutingQueryHandler : IRequestHandler<GetAvailableQuestionsForRoutingQuery, BaseMediatrResponse<GetAvailableQuestionsForRoutingQueryResponse>>
    {
        private readonly IApiClient _apiClient;


        public GetAvailableQuestionsForRoutingQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;

        }

        public async Task<BaseMediatrResponse<GetAvailableQuestionsForRoutingQueryResponse>> Handle(GetAvailableQuestionsForRoutingQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetAvailableQuestionsForRoutingQueryResponse>();
            response.Success = false;
            try
            {
                var questions = await _apiClient.Get<GetAvailableQuestionsForRoutingQueryResponse>(new GetAvailableQuestionsForRoutingApiRequest()
                {
                    FormVersionId = request.FormVersionId,
                    PageId = request.PageId,
                    SectionId = request.SectionId,
                });

                response.Value = questions;
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
