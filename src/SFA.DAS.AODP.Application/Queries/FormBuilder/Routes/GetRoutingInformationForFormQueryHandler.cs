using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Routes;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Routes
{
    public class GetRoutingInformationForFormQueryHandler : IRequestHandler<GetRoutingInformationForFormQuery, BaseMediatrResponse<GetRoutingInformationForFormQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetRoutingInformationForFormQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;

        }

        public async Task<BaseMediatrResponse<GetRoutingInformationForFormQueryResponse>> Handle(GetRoutingInformationForFormQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetRoutingInformationForFormQueryResponse>
            {
                Success = false
            };
            try
            {
                var info = await _apiClient.Get<GetRoutingInformationForFormQueryResponse>(new GetRoutesForFormVersionApiRequest()
                {
                    FormVersionId = request.FormVersionId,
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