using AutoMapper;
using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Responses.Pages;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;

public class GetPageByIdQueryHandler : IRequestHandler<GetPageByIdQuery, GetPageByIdQueryResponse>
{
    private readonly IAodpApiClient<AodpApiConfiguration> _apiClient;
    private readonly IMapper _mapper;

    public GetPageByIdQueryHandler(IAodpApiClient<AodpApiConfiguration> apiClient, IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<GetPageByIdQueryResponse> Handle(GetPageByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new GetPageByIdQueryResponse();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetPageByIdApiResponse>(new GetPageByIdApiRequest(request.PageId, request.SectionId));
            response.Data = _mapper.Map<GetPageByIdQueryResponse.Page>(result.Data);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
