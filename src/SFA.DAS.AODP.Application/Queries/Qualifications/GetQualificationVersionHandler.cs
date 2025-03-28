using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationVersionHandler : IRequestHandler<GetQualificationVersionQuery, BaseMediatrResponse<GetQualificationDetailsQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetQualificationVersionHandler(IApiClient repository)
        {
            _apiClient = repository;
        }

        public async Task<BaseMediatrResponse<GetQualificationDetailsQueryResponse>> Handle(GetQualificationVersionQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetQualificationDetailsQueryResponse>();
            try
            {
                var result = await _apiClient.Get<GetQualificationDetailsQueryResponse>(new GetQualificationVersionApiRequest(request.QualificationReference,request.Version));
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


