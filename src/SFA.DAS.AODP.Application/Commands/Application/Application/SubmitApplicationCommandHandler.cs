using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class SubmitApplicationCommandHandler : IRequestHandler<SubmitApplicationCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiCLient;
    public SubmitApplicationCommandHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }


    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(SubmitApplicationCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>
        {
            Success = false
        };

        try
        {
            await _apiCLient.Put<EmptyResponse>(new SubmitApplicationApiRequest()
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
