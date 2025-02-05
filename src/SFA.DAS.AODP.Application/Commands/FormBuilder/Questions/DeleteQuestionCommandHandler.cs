using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Questions;
using SFA.DAS.AODP.Domain.Interfaces;
namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;
public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand, BaseMediatrResponse<DeleteQuestionCommandResponse>>
{
    private readonly IApiClient _apiClient;
    public DeleteQuestionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    public async Task<BaseMediatrResponse<DeleteQuestionCommandResponse>> Handle(DeleteQuestionCommand command, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<DeleteQuestionCommandResponse>()
        {
            Success = false
        };
        try
        {
            var apiRequest = new DeleteQuestionApiRequest(command.QuestionId, command.PageId, command.FormVersionId, command.SectionId)
            {
                Data = command
            };
            await _apiClient.Delete(apiRequest);
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