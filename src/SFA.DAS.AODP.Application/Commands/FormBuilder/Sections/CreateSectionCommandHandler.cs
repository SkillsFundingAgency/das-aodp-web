using AutoMapper;
using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.FormBuilder.Responses.Sections;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;

public class CreateSectionCommandHandler : IRequestHandler<CreateSectionCommand, CreateSectionCommandResponse>
{
    private readonly IApiClient _apiClient;


    public CreateSectionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<CreateSectionCommandResponse> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateSectionCommandResponse()
        {
            Success = false
        };

        try
        {
            var apiRequest = new CreateSectionApiRequest()
            {
                Data = new CreateSectionApiRequest.Section()
                {
                    Description = request.Description,
                    Title = request.Title,

                },
                FormVersionId = request.FormVersionId,
            };

            var result = await _apiClient.PostWithResponseCode<CreateSectionApiResponse>(apiRequest);
            response.Id = result.Id;
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
