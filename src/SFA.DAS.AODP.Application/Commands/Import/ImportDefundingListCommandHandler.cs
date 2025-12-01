using MediatR;
using SFA.DAS.AODP.Domain.Import;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.Import;

public class ImportDefundingListCommandHandler : IRequestHandler<ImportDefundingListCommand, BaseMediatrResponse<ImportDefundingListResponse>>
{
    private readonly IApiClient _apiClient;

    public ImportDefundingListCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }
    public async Task<BaseMediatrResponse<ImportDefundingListResponse>> Handle(ImportDefundingListCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<ImportDefundingListResponse>
        {
            Success = false
        };

        try
        {
            var apiRequest = new ImportDefundingListApiRequest
            {
                Data = request.File!
            };

            var importResponse = await _apiClient.PostWithMultipartFormData<ImportDefundingListResponse>(apiRequest, cancellationToken);

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