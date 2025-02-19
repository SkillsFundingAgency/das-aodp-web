using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.Cache;

public class GetApplicationMetadataByIdQueryHandler : IRequestHandler<GetApplicationMetadataByIdQuery, BaseMediatrResponse<GetApplicationMetadataByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;
    private readonly ICacheService _cacheService;
    public GetApplicationMetadataByIdQueryHandler(IApiClient apiClient, ICacheService cacheService)
    {
        _apiClient = apiClient;
        _cacheService = cacheService;
    }

    public async Task<BaseMediatrResponse<GetApplicationMetadataByIdQueryResponse>> Handle(GetApplicationMetadataByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationMetadataByIdQueryResponse>();
        response.Success = false;
        try
        {
            var cacheKey = $"{nameof(GetApplicationMetadataByIdQueryResponse)}_{request.ApplicationId}";

            var fetchFunc = async () => await _apiClient.Get<GetApplicationMetadataByIdQueryResponse>(new GetApplicationMetadataByIdRequest()
            {
                ApplicationId = request.ApplicationId,
            });

            var result = await _cacheService.GetAsync(cacheKey, fetchFunc);
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
