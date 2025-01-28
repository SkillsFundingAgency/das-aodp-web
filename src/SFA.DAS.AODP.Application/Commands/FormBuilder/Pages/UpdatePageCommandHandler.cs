using AutoMapper;
using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, BaseMediatrResponse<UpdatePageCommandResponse>>
{
    private readonly IApiClient _apiClient;


    public UpdatePageCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<UpdatePageCommandResponse>> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<UpdatePageCommandResponse>()
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
                Data = request
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
