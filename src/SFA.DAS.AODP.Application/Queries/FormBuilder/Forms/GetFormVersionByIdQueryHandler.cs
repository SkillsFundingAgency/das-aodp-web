using AutoMapper;
using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.FormBuilder.Responses.Forms;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;

public class GetFormVersionByIdQueryHandler : IRequestHandler<GetFormVersionByIdQuery, GetFormVersionByIdQueryResponse>
{
    private readonly IApiClient _apiClient;


    public GetFormVersionByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<GetFormVersionByIdQueryResponse> Handle(GetFormVersionByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new GetFormVersionByIdQueryResponse();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetFormVersionByIdApiResponse>(new GetFormVersionByIdApiRequest(request.FormVersionId));
            response.Data = result.Data;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
