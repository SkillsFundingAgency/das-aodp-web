namespace SFA.DAS.AODP.Application.Queries.Rollover
{
    public class GetRolloverStartSummaryQueryHandler : IRequestHandler<GetRolloverStartSummaryQuery, BaseMediatrResponse<GetRolloverStartSummaryQueryResponse>>
    {
        private readonly IApiClient _apiClient;


        public GetRolloverStartSummaryQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetRolloverStartSummaryQueryResponse>> Handle(GetRolloverStartSummaryQuery request, CancellationToken cancellationToken)
        {
            
            var response = new BaseMediatrResponse<GetRolloverStartSummaryQueryResponse>();

            try
            {
                var result = await _apiClient.Get<GetRolloverStartSummaryQueryResponse>(new GetRolloverStartSummaryApiRequest());

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
