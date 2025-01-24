using AutoMapper;
using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.FormBuilder.Responses.Forms;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class CreateFormVersionCommandHandler : IRequestHandler<CreateFormVersionCommand, CreateFormVersionCommandResponse>
{
    private readonly IApiClient _apiClient;


    public CreateFormVersionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<CreateFormVersionCommandResponse> Handle(CreateFormVersionCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateFormVersionCommandResponse
        {
            Success = false
        };

        try
        {
            var apiRequest = new CreateFormVersionApiRequest()
            {
                Data = new CreateFormVersionApiRequest.FormVersion()
                {
                    Description = request.Description,
                    Title = request.Title
                }
            };
            var result = await _apiClient.PostWithResponseCode<CreateFormVersionApiResponse>(apiRequest);
            response.Id = result.Id!;
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
