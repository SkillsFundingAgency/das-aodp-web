using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;
using SFA.DAS.AODP.Infrastructure.ApiClient;

namespace SFA.DAS.AODP.Application.Commands.Qualification;

public class AddQualificationDiscussionHistoryCommandHandler : IRequestHandler<AddQualificationDiscussionHistoryCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiClient;

    public AddQualificationDiscussionHistoryCommandHandler(IApiClient apiCLient)
    {
        _apiClient = apiCLient;
    }

    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(AddQualificationDiscussionHistoryCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>();

        try
        {
            await _apiClient.PostWithResponseCode<EmptyResponse>(new AddQualificationDiscussionHistoryApiRequest(request));

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
