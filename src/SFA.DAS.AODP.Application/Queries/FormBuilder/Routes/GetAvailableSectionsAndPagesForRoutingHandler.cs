using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Routes
{
    public class GetAvailableSectionsAndPagesForRoutingQueryHandler : IRequestHandler<GetAvailableSectionsAndPagesForRoutingQuery, BaseMediatrResponse<GetAvailableSectionsAndPagesForRoutingQueryResponse>>
    {
        private readonly IApiClient _apiClient;


        public GetAvailableSectionsAndPagesForRoutingQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;

        }

        public async Task<BaseMediatrResponse<GetAvailableSectionsAndPagesForRoutingQueryResponse>> Handle(GetAvailableSectionsAndPagesForRoutingQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetAvailableSectionsAndPagesForRoutingQueryResponse>();
            response.Success = false;
            try
            {
                var sections = await _apiClient.Get<GetAvailableSectionsAndPagesForRoutingQueryResponse>(new GetAvailableSectionsAndPagesForRoutingApiRequest()
                {
                    FormVersionId = request.FormVersionId,
                });
                response.Value = sections;
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
