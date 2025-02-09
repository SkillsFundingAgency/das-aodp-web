using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

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
                new GetQualificationDetailsApiRequest(request.QualificationReference))
                ?? new GetQualificationDetailsQueryResponse { Success = false };
        }
    }
}
