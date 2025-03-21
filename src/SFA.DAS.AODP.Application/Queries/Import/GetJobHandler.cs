using MediatR;
using SFA.DAS.AODP.Domain.Import;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Import
{
    public class GetJobHandler : IRequestHandler<GetJobQuery, BaseMediatrResponse<GetJobResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetJobHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetJobResponse>> Handle(GetJobQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetJobResponse>();
            response.Success = false;
            var jobName = request.JobName;

            try
            {
                var result = await _apiClient.Get<GetJobResponse>(new GetJobByNameApiRequest(jobName));
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
