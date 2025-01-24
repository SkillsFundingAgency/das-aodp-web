using AutoMapper;
using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class UpdateFormVersionCommandHandler : IRequestHandler<UpdateFormVersionCommand, UpdateFormVersionCommandResponse>
{
    private readonly IApiClient _apiClient;


    public UpdateFormVersionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<UpdateFormVersionCommandResponse> Handle(UpdateFormVersionCommand request, CancellationToken cancellationToken)
    {
        var response = new UpdateFormVersionCommandResponse();
        response.Success = false;

        try
        {
            var apiRequest = new UpdateFormVersionApiRequest()
            {
                Data = new UpdateFormVersionApiRequest.FormVersion()
                {
                    Description = request.Description,
                    Name = request.Name,
                    Order = request.Order,
                },
                FormVersionId = request.FormVersionId,
            };
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
