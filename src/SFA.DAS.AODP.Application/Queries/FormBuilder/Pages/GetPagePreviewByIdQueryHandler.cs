using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;

public class GetPagePreviewByIdQueryHandler : IRequestHandler<GetPagePreviewByIdQuery, BaseMediatrResponse<GetPagePreviewByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;


    public GetPagePreviewByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<GetPagePreviewByIdQueryResponse>> Handle(GetPagePreviewByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetPagePreviewByIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetPagePreviewByIdQueryResponse>(new GetPagePreviewByIdApiRequest()
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