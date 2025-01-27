using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;

public class DeleteSectionCommandHandler : IRequestHandler<DeleteSectionCommand, DeleteSectionCommandResponse>
{
    private readonly IApiClient _apiClient;

    public DeleteSectionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<DeleteSectionCommandResponse> Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
    {
        var response = new DeleteSectionCommandResponse()
        {
            Success = false
        };

        try
        {
            await _apiClient.Delete(new DeleteSectionApiRequest()
            {
                SectionId = request.SectionId,
                FormVersionId = request.FormVersionId
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
