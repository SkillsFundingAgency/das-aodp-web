using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationDetailsQueryHandler : IRequestHandler<GetQualificationDetailsQuery, GetQualificationDetailsQueryResponse>
    {
        private readonly IApiClient _apiClient;

        public GetQualificationDetailsQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<GetQualificationDetailsQueryResponse> Handle(GetQualificationDetailsQuery request, CancellationToken cancellationToken)
        {
            return await _apiClient.Get<GetQualificationDetailsQueryResponse>(
                new GetQualificationDetailsApiRequest(request.Id))
                ?? new GetQualificationDetailsQueryResponse { Success = false };
        }
    }
}
