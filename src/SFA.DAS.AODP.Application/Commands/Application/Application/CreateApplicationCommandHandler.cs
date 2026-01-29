using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Application.Application;

public class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, BaseMediatrResponse<CreateApplicationCommandResponse>>
{
    private readonly IApiClient _apiCLient;
    public CreateApplicationCommandHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }


    public async Task<BaseMediatrResponse<CreateApplicationCommandResponse>> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<CreateApplicationCommandResponse>
        {
            Success = false,
            Value = new CreateApplicationCommandResponse()
        };

        try
        {
            var result = await _apiCLient.PostWithResponseCode<CreateApplicationCommandResponse>(new CreateApplicationApiRequest()
            {
                Data = request
            });

            response.Value.Id = result.Id;
            response.Value.IsQanValid = result.IsQanValid;
            response.Value.QanValidationMessage = result.QanValidationMessage;

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