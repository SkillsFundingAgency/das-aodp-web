using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.QaaDownload;

namespace SFA.DAS.AODP.Application.Queries.QaaDownload
{
    public class GetQaaDownloadSummaryQueryHandler : IRequestHandler<GetQaaDownloadSummaryQuery, BaseMediatrResponse<GetQaaDownloadSummaryQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetQaaDownloadSummaryQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetQaaDownloadSummaryQueryResponse>> Handle(GetQaaDownloadSummaryQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetQaaDownloadSummaryQueryResponse>();

            try
            {
                var result = await _apiClient.Get<GetQaaDownloadSummaryQueryResponse>(new GetQaaDownloadSummaryApiRequest());

                response.Value = result;
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
