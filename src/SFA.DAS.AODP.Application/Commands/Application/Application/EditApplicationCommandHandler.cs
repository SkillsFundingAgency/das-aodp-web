using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Application.Application;

public class EditApplicationCommandHandler : IRequestHandler<EditApplicationCommand, BaseMediatrResponse<EditApplicationCommandResponse>>
{
    private readonly IApiClient _apiCLient;
    public EditApplicationCommandHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }

    public async Task<BaseMediatrResponse<EditApplicationCommandResponse>> Handle(EditApplicationCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EditApplicationCommandResponse>
        {
            Success = false,
            Value = new EditApplicationCommandResponse()
        };

        try
        {
            var result = await _apiCLient.PutWithResponseCode<CreateApplicationCommandResponse>(new EditApplicationApiRequest()
            {
                ApplicationId = request.ApplicationId,
                Data = request
            });

            response.Value.IsQanValid = result.Body.IsQanValid;
            response.Value.QanValidationMessage = result.Body.QanValidationMessage;
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
