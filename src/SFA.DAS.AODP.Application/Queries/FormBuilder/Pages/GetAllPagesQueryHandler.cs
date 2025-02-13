using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;

public class GetAllPagesQueryHandler : IRequestHandler<GetAllPagesQuery, BaseMediatrResponse<GetAllPagesQueryResponse>>
{
    private readonly IApiClient _apiClient;


    public GetAllPagesQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<GetAllPagesQueryResponse>> Handle(GetAllPagesQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetAllPagesQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetAllPagesQueryResponse>(new GetAllPagesApiRequest()
            {
                FormVersionId = request.FormVersionId,
                SectionId = request.SectionId
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