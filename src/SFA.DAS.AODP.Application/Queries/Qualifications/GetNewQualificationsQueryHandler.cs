using MediatR;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Test
{
    public class GetNewQualificationsQueryHandler : IRequestHandler<GetNewQualificationsQuery, GetNewQualificationsQueryResponse>
    {
        private readonly IApiClient _apiClient;

        public GetNewQualificationsQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<GetNewQualificationsQueryResponse> Handle(GetNewQualificationsQuery request, CancellationToken cancellationToken)
        {
            return await _apiClient.Get<GetNewQualificationsQueryResponse>(
                new GetNewQualificationsApiRequest())
                ?? new GetNewQualificationsQueryResponse { Success = false };
        }
    }
}
