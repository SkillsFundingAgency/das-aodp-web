using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Questions;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;

public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand, BaseMediatrResponse<UpdateQuestionCommandResponse>>
{
    private readonly IApiClient _apiClient;


    public UpdateQuestionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<UpdateQuestionCommandResponse>> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<UpdateQuestionCommandResponse>()
        {
            Success = false
        };

        try
        {
            var apiRequest = new UpdateQuestionApiRequest(request.Id, request.PageId, request.FormVersionId, request.SectionId)
            {
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
