using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationDetailsQueryHandler : IRequestHandler<GetQualificationDetailsQuery, BaseMediatrResponse<GetQualificationDetailsQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetQualificationDetailsQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetQualificationDetailsQueryResponse>> Handle(GetQualificationDetailsQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetQualificationDetailsQueryResponse>();
            response.Success = false;
            try
            {
                var result = await _apiClient.Get<GetQualificationDetailsQueryResponse>(new GetQualificationDetailsApiRequest(request.QualificationReference));
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
