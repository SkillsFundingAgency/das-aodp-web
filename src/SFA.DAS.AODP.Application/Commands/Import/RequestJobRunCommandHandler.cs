using MediatR;
using SFA.DAS.AODP.Domain.Import;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Import;

public class RequestJobRunCommandHandler : IRequestHandler<RequestJobRunCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiClient;

    public RequestJobRunCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(RequestJobRunCommand command, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>();
        response.Success = false;

        try
        {
            var result = await _apiClient.PostWithResponseCode<EmptyResponse>(new RequestJobRunApiRequest()
            {
                Data = command
            });            
           
            response.Success = true;

        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}

