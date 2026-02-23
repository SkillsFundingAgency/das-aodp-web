using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Commands.Qualifications
{
    public class BulkUpdateQualificationStatusCommandHandler : IRequestHandler<BulkUpdateQualificationStatusCommand, BaseMediatrResponse<BulkUpdateQualificationsStatusResponse>>
    {
        private readonly IApiClient _apiClient;


        public BulkUpdateQualificationStatusCommandHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<BulkUpdateQualificationsStatusResponse>> Handle(BulkUpdateQualificationStatusCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<BulkUpdateQualificationsStatusResponse>()
            {
                Success = false
            };

            try
            {
                var apiRequest = new BulkUpdateQualificationStatusApiRequest()
                {
                    Data = request
                };
                //await _apiClient.Put(apiRequest);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.Success = false;
            }

            return response;
        }

    }
}
