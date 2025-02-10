using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class MoveFormDownCommandHandler : IRequestHandler<MoveFormDownCommand, BaseMediatrResponse<MoveFormDownCommandResponse>>
{
    private readonly IApiClient _apiClient;

    public MoveFormDownCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<MoveFormDownCommandResponse>> Handle(MoveFormDownCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<MoveFormDownCommandResponse>();
        response.Success = false;

        try
        {
            var apiRequest = new MoveFormDownApiRequest(request.FormId);
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
