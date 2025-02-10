using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Routes;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Routes
{
    public class GetRoutingInformationForQuestionQueryHandler : IRequestHandler<GetRoutingInformationForQuestionQuery, BaseMediatrResponse<GetRoutingInformationForQuestionQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetRoutingInformationForQuestionQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;

        }

        public async Task<BaseMediatrResponse<GetRoutingInformationForQuestionQueryResponse>> Handle(GetRoutingInformationForQuestionQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetRoutingInformationForQuestionQueryResponse>
            {
                Success = false
            };
            try
            {
                var info = await _apiClient.Get<GetRoutingInformationForQuestionQueryResponse>(new GetRoutingInformationForQuestionApiRequest()
                {
                    FormVersionId = request.FormVersionId,
                    PageId = request.PageId,
                    QuestionId = request.QuestionId,
                    SectionId = request.SectionId
                });

                response.Value = info;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }

            return response;
        }
    }
}
