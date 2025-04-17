using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationDetailWithVersionsQueryHandler : IRequestHandler<GetQualificationDetailWithVersionsQuery, BaseMediatrResponse<GetQualificationDetailsQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetQualificationDetailWithVersionsQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetQualificationDetailsQueryResponse>> Handle(GetQualificationDetailWithVersionsQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetQualificationDetailsQueryResponse>();
            try
            {
                var result = await _apiClient.Get<GetQualificationDetailsQueryResponse>(new GetQualificationDetailWithVersionsApiRequest(request.QualificationReference));
                if (result != null)
                {
                    response.Value = result;
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.ErrorMessage = $"No details found for qualification reference: {request.QualificationReference}";
                }
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

