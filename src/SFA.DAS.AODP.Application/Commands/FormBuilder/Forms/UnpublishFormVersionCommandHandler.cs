using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class UnpublishFormVersionCommandHandler : IRequestHandler<UnpublishFormVersionCommand, BaseMediatrResponse<UnpublishFormVersionCommandResponse>>
{
    private readonly IApiClient _apiClient;

    public UnpublishFormVersionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<UnpublishFormVersionCommandResponse>> Handle(UnpublishFormVersionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<UnpublishFormVersionCommandResponse>();
        response.Success = false;

        try
        {
            var apiRequest = new UnpublishFormVersionApiRequest(request.FormVersionId);
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
