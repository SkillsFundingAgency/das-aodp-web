using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.QaaDownload;

namespace SFA.DAS.AODP.Application.Queries.QaaDownload
{
    public class GetQaaQualificationsExportQueryHandler : IRequestHandler<GetQaaQualificationsExportQuery, BaseMediatrResponse<GetQaaQualificationsExportQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetQaaQualificationsExportQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetQaaQualificationsExportQueryResponse>> Handle(GetQaaQualificationsExportQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetQaaQualificationsExportQueryResponse>();

            try
            {
                var result = await _apiClient.Get<GetQaaQualificationsExportQueryResponse>(new GetQaaQualificationsExportApiRequest
                {
                    CurrentUsername = request.CurrentUsername
                });

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
