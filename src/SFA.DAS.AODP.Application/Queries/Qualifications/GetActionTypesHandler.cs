using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{

    public class GetActionTypes : IRequestHandler<GetActionTypesQuery, BaseMediatrResponse<GetActionTypesResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetActionTypes(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetActionTypesResponse>> Handle(GetActionTypesQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetActionTypesResponse>();
            try
            {
                var result = await _apiClient.Get< BaseMediatrResponse < GetActionTypesResponse >>(new GetActionTypesApiRequest());
                if (result != null)
                {
                    response.Value = result.Value;
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.ErrorMessage = $"No Action Types";
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


