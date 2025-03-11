using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.Cache;

namespace SFA.DAS.AODP.Application.Queries.Application.Form;
public class GetApplicationFormByIdQueryHandler : IRequestHandler<GetApplicationFormByIdQuery, BaseMediatrResponse<GetApplicationFormByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;
    private readonly ICacheService _cacheService;


    public GetApplicationFormByIdQueryHandler(IApiClient apiClient, ICacheService cacheService)
    {
        _apiClient = apiClient;
        _cacheService = cacheService;
    }

    public async Task<BaseMediatrResponse<GetApplicationFormByIdQueryResponse>> Handle(GetApplicationFormByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationFormByIdQueryResponse>();
        response.Success = false;
        try
        {
            var cacheKey = $"{nameof(GetApplicationFormByIdQueryResponse)}_{request.FormVersionId}";

            var fetchFunc = async () => await _apiClient.Get<GetApplicationFormByIdQueryResponse>(new GetApplicationFormByIdApiRequest()
            {
                FormVersionId = request.FormVersionId,
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