using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationMetadataByIdQueryHandler : IRequestHandler<GetApplicationMetadataByIdQuery, BaseMediatrResponse<GetApplicationMetadataByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetApplicationMetadataByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetApplicationMetadataByIdQueryResponse>> Handle(GetApplicationMetadataByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationMetadataByIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetApplicationMetadataByIdQueryResponse>(new GetApplicationMetadataByIdRequest()
            {
                ApplicationId = request.ApplicationId,
            });
            response.Value = result;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
