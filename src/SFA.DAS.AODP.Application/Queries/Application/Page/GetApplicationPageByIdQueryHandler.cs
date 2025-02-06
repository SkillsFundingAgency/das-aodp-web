using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationPageByIdQueryHandler : IRequestHandler<GetApplicationPageByIdQuery, BaseMediatrResponse<GetApplicationPageByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;


    public GetApplicationPageByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<GetApplicationPageByIdQueryResponse>> Handle(GetApplicationPageByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationPageByIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetApplicationPageByIdQueryResponse>(new GetApplicationPageByIdApiRequest()
            {
                FormVersionId = request.FormVersionId,
                SectionId = request.SectionId,
                PageOrder = request.PageOrder
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
