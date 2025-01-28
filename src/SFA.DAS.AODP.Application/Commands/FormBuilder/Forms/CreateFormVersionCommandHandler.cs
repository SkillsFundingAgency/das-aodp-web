using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class CreateFormVersionCommandHandler : IRequestHandler<CreateFormVersionCommand, BaseMediatrResponse<CreateFormVersionCommandResponse>>
{
    private readonly IApiClient _apiClient;


    public CreateFormVersionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<CreateFormVersionCommandResponse>> Handle(CreateFormVersionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<CreateFormVersionCommandResponse>
        {
            Success = false
        };

        try
        {
            var apiRequest = new CreateFormVersionApiRequest()
            {
                Data = request
            };
            var result = await _apiClient.PostWithResponseCode<CreateFormVersionCommandResponse>(apiRequest);
            response.Value.Id = result.Id!;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response!.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
