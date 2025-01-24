using AutoMapper;
using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, UpdatePageCommandResponse>
{
    private readonly IApiClient _apiClient;
    

    public UpdatePageCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
       
    }

    public async Task<UpdatePageCommandResponse> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var response = new UpdatePageCommandResponse()
        {
            Success = false
        };

        try
        {
            var apiRequest = new UpdatePageApiRequest()
            {
                PageId = request.Id,
                FormVersionId = request.FormVersionId,
                SectionId = request.SectionId,
                Data = new UpdatePageApiRequest.Page()
                {
                    Description = request.Description,
                    Title = request.Title

                }
            };
            await _apiClient.Put(apiRequest);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            response.Success = false;
        }

        return response;
    }
}
