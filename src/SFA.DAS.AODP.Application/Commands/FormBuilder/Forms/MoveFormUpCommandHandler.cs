using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class MoveFormUpCommandHandler : IRequestHandler<MoveFormUpCommand, BaseMediatrResponse<MoveFormUpCommandResponse>>
{
    private readonly IApiClient _apiClient;

    public MoveFormUpCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<MoveFormUpCommandResponse>> Handle(MoveFormUpCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<MoveFormUpCommandResponse>();
        response.Success = false;

        try
        {
            var apiRequest = new MoveFormUpApiRequest(request.FormVersionId);
            await _apiClient.Put(apiRequest);
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
