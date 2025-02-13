﻿using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;

public class GetAllSectionsQueryHandler : IRequestHandler<GetAllSectionsQuery, BaseMediatrResponse<GetAllSectionsQueryResponse>>
{
    private readonly IApiClient _apiClient;


    public GetAllSectionsQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<GetAllSectionsQueryResponse>> Handle(GetAllSectionsQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetAllSectionsQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetAllSectionsQueryResponse>(new GetAllSectionsApiRequest()
            {
                FormVersionId = request.FormVersionId,
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