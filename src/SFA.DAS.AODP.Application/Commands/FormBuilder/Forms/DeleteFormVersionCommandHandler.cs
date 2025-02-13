using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class DeleteFormVersionCommandHandler : IRequestHandler<DeleteFormVersionCommand, BaseMediatrResponse<DeleteFormVersionCommandResponse>>
{
    private readonly IApiClient _apiClient;

    public DeleteFormVersionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<DeleteFormVersionCommandResponse>> Handle(DeleteFormVersionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<DeleteFormVersionCommandResponse>();
        response.Success = false;

        try
        {
            var apiRequest = new DeleteFormVersionApiRequest(request.FormVersionId);
            await _apiClient.Delete(apiRequest);
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