using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Queries.Rollover
{
    public class GetRolloverCandidatesForExportQueryHandler : IRequestHandler<GetRolloverCandidatesForExportQuery, BaseMediatrResponse<GetRolloverCandidatesForExportQueryResponse>>
    {
        private readonly IApiClient _apiClient;


        public GetRolloverCandidatesForExportQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetRolloverCandidatesForExportQueryResponse>> Handle(GetRolloverCandidatesForExportQuery request, CancellationToken cancellationToken)
        {
            
            var response = new BaseMediatrResponse<GetRolloverCandidatesForExportQueryResponse>();

            try
            {
                var result = await _apiClient.Get<GetRolloverCandidatesForExportQueryResponse>(new GetRolloverCandidatesForExportApiRequest()
                {
                    RolloverWorkflowRunId = request.RolloverWorkflowRunId
                });

                response.Value = result;
                response.Success = true;
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
