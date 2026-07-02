using MediatR;
using SFA.DAS.AODP.Domain.Application.Review;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Application.Review
{
    public class GetApplicationExportDetailsQueryHandler :IRequestHandler<GetApplicationExportDataQuery, BaseMediatrResponse<GetApplicationExportDataQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetApplicationExportDetailsQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetApplicationExportDataQueryResponse>> Handle(GetApplicationExportDataQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetApplicationExportDataQueryResponse>();
            response.Success = false;
            try
            {
                var result = await _apiClient.Get<GetApplicationExportDataQueryResponse>(
                    new GetApplicationExportDetailsApiRequest(request.ApplicationReviewId));

                response.Value = result;
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
