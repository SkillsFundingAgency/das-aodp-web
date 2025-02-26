using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.Cache;

public class GetApplicationPageByIdQueryHandler : IRequestHandler<GetApplicationPageByIdQuery, BaseMediatrResponse<GetApplicationPageByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;
    private readonly ICacheService _cacheService;


    public GetApplicationPageByIdQueryHandler(IApiClient apiClient, ICacheService cacheService)
    {
        _apiClient = apiClient;
        _cacheService = cacheService;
    }

    public async Task<BaseMediatrResponse<GetApplicationPageByIdQueryResponse>> Handle(GetApplicationPageByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationPageByIdQueryResponse>();
        response.Success = false;
        try
        {
            var cacheKey = $"{nameof(GetApplicationPageByIdQueryResponse)}_{request.FormVersionId}_{request.SectionId}_{request.PageOrder}";
            var fetchFunc = async () => await _apiClient.Get<GetApplicationPageByIdQueryResponse>(new GetApplicationPageByIdApiRequest()
            {
                FormVersionId = request.FormVersionId,
                SectionId = request.SectionId,
                PageOrder = request.PageOrder
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
