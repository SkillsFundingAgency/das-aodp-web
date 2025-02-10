using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class CreateDraftFormVersionCommandHandler : IRequestHandler<CreateDraftFormVersionCommand, BaseMediatrResponse<CreateDraftFormVersionCommandResponse>>
{
    private readonly IApiClient _apiClient;

    public CreateDraftFormVersionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<CreateDraftFormVersionCommandResponse>> Handle(CreateDraftFormVersionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<CreateDraftFormVersionCommandResponse>
        {
            Success = false
        };

        try
        {
            var apiRequest = new CreateDraftFormVersionApiRequest(request.FormId);
            response.Value = await _apiClient.Put<CreateDraftFormVersionCommandResponse>(apiRequest);
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
