using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Questions;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Application.Page;

public class UpdatePageAnswersCommandHandler : IRequestHandler<UpdatePageAnswersCommand, BaseMediatrResponse<UpdatePageAnswersCommandResponse>>
{
    private readonly IApiClient _apiClient;


    public UpdatePageAnswersCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<UpdatePageAnswersCommandResponse>> Handle(UpdatePageAnswersCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<UpdatePageAnswersCommandResponse>()
        {
            Success = false
        };

        try
        {
            var apiRequest = new UpdatePageAnswersApiRequest(request.ApplicationId, request.PageId, request.FormVersionId, request.SectionId)
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
