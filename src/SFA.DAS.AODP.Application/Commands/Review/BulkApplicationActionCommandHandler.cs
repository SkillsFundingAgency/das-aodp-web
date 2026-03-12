
using MediatR;
using SFA.DAS.AODP.Domain.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Review;

public class BulkApplicationActionCommandHandler : IRequestHandler<BulkApplicationActionCommand, BaseMediatrResponse<BulkApplicationActionCommandResponse>>
{
    private readonly IApiClient _apiClient;
    public BulkApplicationActionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<BulkApplicationActionCommandResponse>> Handle(BulkApplicationActionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<BulkApplicationActionCommandResponse>()
        {
            Success = false
        };

        try
        {
            var apiResult = await _apiClient.PutWithResponseCode<BulkApplicationActionCommandResponse>(
                new BulkApplicationActionApiRequest()
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
