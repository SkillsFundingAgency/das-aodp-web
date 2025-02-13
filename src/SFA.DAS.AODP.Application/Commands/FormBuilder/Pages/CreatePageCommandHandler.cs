using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class CreatePageCommandHandler : IRequestHandler<CreatePageCommand, BaseMediatrResponse<CreatePageCommandResponse>>
{
    private readonly IApiClient _apiClient;
    

    public CreatePageCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
       
    }

    public async Task<BaseMediatrResponse<CreatePageCommandResponse>> Handle(CreatePageCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<CreatePageCommandResponse>();
        try
        {
            var apiRequestData = new CreatePageApiRequest()
            {
                Data = request,
                SectionId = request.SectionId,
                FormVersionId = request.FormVersionId
            };

            var result = await _apiClient.PostWithResponseCode<CreatePageCommandResponse>(apiRequestData);
            response.Value.Id = result!.Id;
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
