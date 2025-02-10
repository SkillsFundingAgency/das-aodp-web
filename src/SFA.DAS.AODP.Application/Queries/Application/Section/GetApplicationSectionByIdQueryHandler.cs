using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationSectionByIdQueryHandler : IRequestHandler<GetApplicationSectionByIdQuery, BaseMediatrResponse<GetApplicationSectionByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetApplicationSectionByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetApplicationSectionByIdQueryResponse>> Handle(GetApplicationSectionByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationSectionByIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetApplicationSectionByIdQueryResponse>(new GetApplicationSectionByIdApiRequest()
            {
                SectionId = request.SectionId,
                FormVersionId = request.FormVersionId,
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
