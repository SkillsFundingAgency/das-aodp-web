using AutoMapper;
using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Responses.Pages;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class CreatePageCommandHandler : IRequestHandler<CreatePageCommand, CreatePageCommandResponse>
{
    private readonly IApiClient _apiClient;
    

    public CreatePageCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
       
    }

    public async Task<CreatePageCommandResponse> Handle(CreatePageCommand request, CancellationToken cancellationToken)
    {
        var response = new CreatePageCommandResponse();
        try
        {
            var apiRequestData = new CreatePageApiRequest()
            {
                Data = new CreatePageApiRequest.Page()
                {
                    Description = request.Description,
                    Title = request.Title
                },
                SectionId = request.SectionId,
                FormVersionId = request.FormVersionId
            };

            var result = await _apiClient.PostWithResponseCode<CreatePageApiResponse>(apiRequestData);
            response.Id = result!.Id;
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
