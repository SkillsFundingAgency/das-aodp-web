using MediatR;
using SFA.DAS.AODP.Domain.Import;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Import
{
    public class GetJobRunByIdQueryHandler : IRequestHandler<GetJobRunByIdQuery, BaseMediatrResponse<GetJobRunByIdQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetJobRunByIdQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetJobRunByIdQueryResponse>> Handle(GetJobRunByIdQuery query, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetJobRunByIdQueryResponse>();
            response.Success = false;
            try
            {
                var result = await _apiClient.Get<GetJobRunByIdQueryResponse>(new GetJobRunByIdApiRequest(query.Id));               

                response.Value = result;
                response.Success = true;

            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }

            return response;
        }
    }
}
