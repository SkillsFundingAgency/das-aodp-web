using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Questions;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;

public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, BaseMediatrResponse<CreateQuestionCommandResponse>>
{
    private readonly IApiClient _apiClient;


    public CreateQuestionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<CreateQuestionCommandResponse>> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<CreateQuestionCommandResponse>();
        try
        {
            var apiRequestData = new CreateQuestionApiRequest()
            {
                Data = request,
                SectionId = request.SectionId,
                FormVersionId = request.FormVersionId,
                PageId = request.PageId
            };

            var result = await _apiClient.PostWithResponseCode<CreateQuestionCommandResponse>(apiRequestData);
            response.Value.Id = result!.Id;
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
