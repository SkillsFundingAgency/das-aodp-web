using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetChangedQualificationDetailsHandler : IRequestHandler<GetChangedQualificationDetailsQuery, BaseMediatrResponse<GetChangedQualificationDetailsResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetChangedQualificationDetailsHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetChangedQualificationDetailsResponse>> Handle(GetChangedQualificationDetailsQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetChangedQualificationDetailsResponse>();
            try
            {
                var result = await _apiClient.Get<GetChangedQualificationDetailsResponse>(new GetChangedQualificationDetailsApiRequest(request.QualificationReference,request.Status));
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

