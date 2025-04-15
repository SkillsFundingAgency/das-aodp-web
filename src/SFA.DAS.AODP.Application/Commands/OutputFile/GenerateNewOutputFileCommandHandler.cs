using MediatR;
using SFA.DAS.AODP.Application.Queries.OutputFile;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.OutputFile;

namespace SFA.DAS.AODP.Application.Commands.OutputFile;

public class GenerateNewOutputFileCommandHandler : IRequestHandler<GenerateNewOutputFileCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiClient;


    public GenerateNewOutputFileCommandHandler(IApiClient aodpApiClient)
    {
        _apiClient = aodpApiClient;

    }

    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(GenerateNewOutputFileCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>
        {
            Success = false
        };

        try
        {
            var result = await _apiClient.PostWithResponseCode<EmptyResponse>(new GenerateNewOutputFileApiRequest()
            {
                Data = request
            });
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