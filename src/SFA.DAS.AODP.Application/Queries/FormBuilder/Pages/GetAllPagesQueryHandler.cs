using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Responses.Pages;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;

public class GetAllPagesQueryHandler : IRequestHandler<GetAllPagesQuery, GetAllPagesQueryResponse>
{
    private readonly IApiClient _apiClient;


    public GetAllPagesQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<GetAllPagesQueryResponse> Handle(GetAllPagesQuery request, CancellationToken cancellationToken)
    {
        var response = new GetAllPagesQueryResponse();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetAllPagesApiResponse>(new GetAllPagesApiRequest()
            {
                FormVersionId = request.FormVersionId,
                SectionId = request.SectionId
            });
            response.Data = [.. result.Data];
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}