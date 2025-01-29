using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class DeletePageCommandHandler : IRequestHandler<DeletePageCommand, BaseMediatrResponse<DeletePageCommandResponse>>
{
    private readonly IApiClient _apiClient;

    public DeletePageCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<DeletePageCommandResponse>> Handle(DeletePageCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<DeletePageCommandResponse>();

        try
        {
            await _apiClient.Delete(new DeletePageApiRequest()
            {
                FormVersionId = request.FormVersionId,
                PageId = request.PageId,
                SectionId = request.SectionId
            });
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
