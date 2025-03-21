﻿using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Application.Application;

public class GetApplicationByIdQueryHandler : IRequestHandler<GetApplicationByIdQuery, BaseMediatrResponse<GetApplicationByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;
    public GetApplicationByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetApplicationByIdQueryResponse>> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationByIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetApplicationByIdQueryResponse>(new GetApplicationByIdRequest()
            {
                ApplicationId = request.ApplicationId,
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
