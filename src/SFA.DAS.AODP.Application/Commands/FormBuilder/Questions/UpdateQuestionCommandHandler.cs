using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Questions;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;

public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand, UpdateQuestionCommandResponse>
{
    private readonly IApiClient _apiClient;


    public UpdateQuestionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<UpdateQuestionCommandResponse> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var response = new UpdateQuestionCommandResponse()
        {
            Success = false
        };

        try
        {
            var apiRequest = new UpdateQuestionApiRequest()
            {
                QuestionId = request.Id,
                PageId = request.PageId,
                FormVersionId = request.FormVersionId,
                SectionId = request.SectionId,
                Data = new UpdateQuestionApiRequest.Question()
                {
                    Hint = request.Hint,
                    Title = request.Title,
                    Required = request.Required,
                    TextInput = new()
                    {
                        MinLength = request.TextInput.MinLength,
                        MaxLength = request.TextInput.MaxLength,
                    }
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
