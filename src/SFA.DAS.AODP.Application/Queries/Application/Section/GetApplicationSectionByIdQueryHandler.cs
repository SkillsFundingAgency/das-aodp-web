using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.Cache;

namespace SFA.DAS.AODP.Application.Queries.Application.Section;
public class GetApplicationSectionByIdQueryHandler : IRequestHandler<GetApplicationSectionByIdQuery, BaseMediatrResponse<GetApplicationSectionByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;
    private readonly ICacheService _cacheService;

    public GetApplicationSectionByIdQueryHandler(IApiClient apiClient, ICacheService cacheService)
    {
        _apiClient = apiClient;
        _cacheService = cacheService;
    }

    public async Task<BaseMediatrResponse<GetApplicationSectionByIdQueryResponse>> Handle(GetApplicationSectionByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationSectionByIdQueryResponse>();
        response.Success = false;
        try
        {
            var cacheKey = $"{nameof(GetApplicationSectionByIdQueryResponse)}_{request.FormVersionId}_{request.SectionId}";
            var fetchFunc = async () => await _apiClient.Get<GetApplicationSectionByIdQueryResponse>(new GetApplicationSectionByIdApiRequest()
            {
                SectionId = request.SectionId,
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
