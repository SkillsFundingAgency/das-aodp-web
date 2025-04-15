using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.OutputFile;

namespace SFA.DAS.AODP.Application.Queries.OutputFile;

public class GetPreviousOutputFilesQueryHandler : IRequestHandler<GetPreviousOutputFilesQuery, BaseMediatrResponse<GetPreviousOutputFilesQueryResponse>>
{
    private readonly IApiClient _apiClient;


    public GetPreviousOutputFilesQueryHandler(IApiClient aodpApiClient)
    {
        _apiClient = aodpApiClient;

    }

    public async Task<BaseMediatrResponse<GetPreviousOutputFilesQueryResponse>> Handle(GetPreviousOutputFilesQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetPreviousOutputFilesQueryResponse>
        {
            Success = false
        };

        try
        {
            var result = await _apiClient.Get<GetPreviousOutputFilesQueryResponse>(new GetPreviousOutputFilesApiRequest());
            response.Value = result;
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