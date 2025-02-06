using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationSectionStatusByApplicationIdQueryHandler : IRequestHandler<GetApplicationSectionStatusByApplicationIdQuery, BaseMediatrResponse<GetApplicationSectionStatusByApplicationIdQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetApplicationSectionStatusByApplicationIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetApplicationSectionStatusByApplicationIdQueryResponse>> Handle(GetApplicationSectionStatusByApplicationIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationSectionStatusByApplicationIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetApplicationSectionStatusByApplicationIdQueryResponse>(new GetApplicationSectionStatusByIdApiRequest()
            {
                FormVersionId = request.FormVersionId,
                ApplicationId = request.ApplicationId,
                SectionId = request.SectionId,
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
