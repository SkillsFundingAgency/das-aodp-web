using MediatR;
using SFA.DAS.AODP.Domain.Import;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Import;

public class ImportPldnsCommandHandler : IRequestHandler<ImportPldnsCommand, BaseMediatrResponse<ImportPldnsCommandResponse>>
{
    private readonly IApiClient _apiClient;

    public ImportPldnsCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<ImportPldnsCommandResponse>> Handle(ImportPldnsCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<ImportPldnsCommandResponse>
        {
            Success = false
        };

        try
        {
            var apiRequest = new ImportPldnsApiRequest
            {
                Data = request.File! 
            };

            var importResponse = await _apiClient.PostWithMultipartFormData<ImportPldnsCommandResponse>(apiRequest, cancellationToken);

            response.Value = importResponse!;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
