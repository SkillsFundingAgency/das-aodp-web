using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Application.Application;

public class EditApplicationCommandHandler : IRequestHandler<EditApplicationCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiCLient;
    public EditApplicationCommandHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }


    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(EditApplicationCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>
        {
            Success = false
        };

        try
        {
            await _apiCLient.Put<CreateApplicationCommandResponse>(new EditApplicationApiRequest()
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
