using AutoMapper;
using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.FormBuilder.Responses.Sections;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;

public class GetAllSectionsQueryHandler : IRequestHandler<GetAllSectionsQuery, GetAllSectionsQueryResponse>
{
    private readonly IApiClient _apiClient;
    private readonly IMapper _mapper;

    public GetAllSectionsQueryHandler(IApiClient apiClient, IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<GetAllSectionsQueryResponse> Handle(GetAllSectionsQuery request, CancellationToken cancellationToken)
    {
        var response = new GetAllSectionsQueryResponse();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetAllSectionsApiResponse>(new GetAllSectionsApiRequest(request.FormVersionId));
            response.Data = _mapper.Map<List<GetAllSectionsQueryResponse.Section>>(result.Data);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}