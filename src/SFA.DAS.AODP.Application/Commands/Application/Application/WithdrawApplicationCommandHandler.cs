using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class WithdrawApplicationCommandHandler : IRequestHandler<WithdrawApplicationCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiClient;
    public WithdrawApplicationCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }


    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(WithdrawApplicationCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>
        {
            Success = false
        };

        try
        {
            await _apiClient.PostWithResponseCode<EmptyResponse>(new WithdrawApplicationApiRequest()
            {
                ApplicationId = request.ApplicationId,
                Data = request
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
