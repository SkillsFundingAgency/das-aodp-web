using Azure;
using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Commands.Qualifications
{
    public class BulkUpdateQualificationStatusCommandHandler : IRequestHandler<BulkUpdateQualificationStatusCommand, BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>>
    {
        private readonly IApiClient _apiClient;

        public BulkUpdateQualificationStatusCommandHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>> Handle(BulkUpdateQualificationStatusCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<BulkUpdateQualificationStatusCommandResponse>()
            {
                Success = false
            };

            try
            {
                var apiResult = await _apiClient.PutWithResponseCode<BulkUpdateQualificationStatusCommandResponse>(
                    new BulkUpdateQualificationStatusApiRequest()
                    {
                        Data = request
                    });

                response.Value = apiResult.Body;
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
