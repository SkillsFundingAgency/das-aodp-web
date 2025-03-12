using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Application.Application;
public class DeleteApplicationCommandHandler : IRequestHandler<DeleteApplicationCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiCLient;
    public DeleteApplicationCommandHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }


    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(DeleteApplicationCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>
        {
            Success = false
        };

        try
        {
            await _apiCLient.Delete(new DeleteApplicationApiRequest()
            {
                ApplicationId = request.ApplicationId
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
