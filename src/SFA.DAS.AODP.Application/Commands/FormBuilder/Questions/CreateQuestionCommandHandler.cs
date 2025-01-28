using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Questions;
using SFA.DAS.AODP.Domain.FormBuilder.Responses.Questions;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;

public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, CreateQuestionCommandResponse>
{
    private readonly IApiClient _apiClient;


    public CreateQuestionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<CreateQuestionCommandResponse> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateQuestionCommandResponse();
        try
        {
            var apiRequestData = new CreateQuestionApiRequest()
            {
                Data = new CreateQuestionApiRequest.Question()
                {
                    Type = request.Type,
                    Title = request.Title,
                    Required = request.Required
                },
                SectionId = request.SectionId,
                FormVersionId = request.FormVersionId,
                PageId = request.PageId
            };

            var result = await _apiClient.PostWithResponseCode<CreateQuestionApiResponse>(apiRequestData);
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
