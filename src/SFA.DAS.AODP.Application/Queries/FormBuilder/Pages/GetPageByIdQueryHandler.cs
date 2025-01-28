using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;

public class GetPageByIdQueryHandler : IRequestHandler<GetPageByIdQuery, BaseMediatrResponse<GetPageByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;


    public GetPageByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<GetPageByIdQueryResponse>> Handle(GetPageByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetPageByIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetPageByIdQueryResponse>(new GetPageByIdApiRequest()
            {
                FormVersionId = request.FormVersionId,
                SectionId = request.SectionId,
                PageId = request.PageId
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
