using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetChangedQualificationDetailsQueryHandler : IRequestHandler<GetChangedQualificationDetailsQuery, BaseMediatrResponse<GetChangedQualificationDetailsQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetChangedQualificationDetailsQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetChangedQualificationDetailsQueryResponse>> Handle(GetChangedQualificationDetailsQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetChangedQualificationDetailsQueryResponse>();
            try
            {
                var result = await _apiClient.Get<BaseMediatrResponse<GetChangedQualificationDetailsQueryResponse>>(new GetChangedQualificationDetailsApiRequest(request.QualificationReference));
                if (result != null && result.Value != null)
                {
                    response.Value = result.Value;
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

