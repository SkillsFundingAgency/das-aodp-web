using MediatR;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Test
{
    public class GetChangedQualificationsQueryHandler : IRequestHandler<GetChangedQualificationsQuery, BaseMediatrResponse<GetChangedQualificationsQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetChangedQualificationsQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetChangedQualificationsQueryResponse>> Handle(GetChangedQualificationsQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetChangedQualificationsQueryResponse>();
            response.Success = false;
            try
            {
                var result = await _apiClient.Get<GetChangedQualificationsQueryResponse>(new GetChangedQualificationsApiRequest()
                {
                    Skip = request.Skip,
                    Take = request.Take,
                    Name = request.Name,
                    Organisation = request.Organisation,
                    QAN = request.QAN,
                });

                if (result != null)
                {
                    response.Value = result;
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }

            return response;
        }
    }
}

