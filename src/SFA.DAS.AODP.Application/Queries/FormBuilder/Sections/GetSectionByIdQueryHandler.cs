using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;

public class GetSectionByIdQueryHandler : IRequestHandler<GetSectionByIdQuery, BaseMediatrResponse<GetSectionByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;


    public GetSectionByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<GetSectionByIdQueryResponse>> Handle(GetSectionByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetSectionByIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetSectionByIdQueryResponse>(new GetSectionByIdApiRequest(request.SectionId, request.FormVersionId));
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
