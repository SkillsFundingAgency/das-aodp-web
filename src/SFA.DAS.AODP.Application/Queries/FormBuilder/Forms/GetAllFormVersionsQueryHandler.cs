using AutoMapper;
using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.FormBuilder.Responses.Forms;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;

public class GetAllFormVersionsQueryHandler : IRequestHandler<GetAllFormVersionsQuery, GetAllFormVersionsQueryResponse>
{
    private readonly IApiClient _apiClient;


    public GetAllFormVersionsQueryHandler(IApiClient aodpApiClient)
    {
        _apiClient = aodpApiClient;

    }

    public async Task<GetAllFormVersionsQueryResponse> Handle(GetAllFormVersionsQuery request, CancellationToken cancellationToken)
    {
        var response = new GetAllFormVersionsQueryResponse
        {
            Success = false
        };

        try
        {
            var result = await _apiClient.Get<GetAllFormVersionsApiResponse>(new GetAllFormVersionsApiRequest());
            response.Data = [.. result.Data];
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            response.Success = false;
        }

        return response;
    }
}