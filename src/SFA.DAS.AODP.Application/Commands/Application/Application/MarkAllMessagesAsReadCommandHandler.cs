using MediatR;
using SFA.DAS.AODP.Domain.Application.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Application.Application;

public class MarkAllMessagesAsReadCommandHandler : IRequestHandler<MarkAllMessagesAsReadCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiClient;
    public MarkAllMessagesAsReadCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(MarkAllMessagesAsReadCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>
        {
            Success = false
        };

        try
        {
            await _apiClient.Put(new MarkAllMessagesAsReadApiRequest()
            {
                ApplicationId = request.ApplicationId,
                Data = request
            });

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
