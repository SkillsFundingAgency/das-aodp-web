using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;

public class CreateSectionCommandHandler : IRequestHandler<CreateSectionCommand, BaseMediatrResponse<CreateSectionCommandResponse>>
{
    private readonly IApiClient _apiClient;


    public CreateSectionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<CreateSectionCommandResponse>> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<CreateSectionCommandResponse>()
        {
            Success = false
        };

        try
        {
            var apiRequest = new CreateSectionApiRequest()
            {
                Data = request,
                FormVersionId = request.FormVersionId,
            };

            var result = await _apiClient.PostWithResponseCode<CreateSectionCommandResponse>(apiRequest);
            response.Value.Id = result.Id;
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
