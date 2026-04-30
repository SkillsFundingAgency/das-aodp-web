
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.AODP.Domain.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Review;

public class BulkApplicationActionCommandHandler : IRequestHandler<BulkApplicationActionCommand, BaseMediatrResponse<BulkApplicationActionCommandResponse>>
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<BulkApplicationActionCommandHandler> _logger;

    public BulkApplicationActionCommandHandler(IApiClient apiClient, 
        ILogger<BulkApplicationActionCommandHandler> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<BaseMediatrResponse<BulkApplicationActionCommandResponse>> Handle(BulkApplicationActionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<BulkApplicationActionCommandResponse>()
        {
            Success = false
        };

        try
        {
            _logger.LogInformation(
                "WEB HANDLER: Sending BulkApplicationAction to outer API. Count={Count}, ActionType={ActionType}, PutUrl={PutUrl}",
                request?.ApplicationReviewIds?.Count,
                request?.ActionType,
                new BulkApplicationActionApiRequest().PutUrl);

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
