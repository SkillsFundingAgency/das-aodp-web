﻿using AutoMapper;
using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class UpdateFormVersionCommandHandler : IRequestHandler<UpdateFormVersionCommand, BaseMediatrResponse<UpdateFormVersionCommandResponse>>
{
    private readonly IApiClient _apiClient;


    public UpdateFormVersionCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<UpdateFormVersionCommandResponse>> Handle(UpdateFormVersionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<UpdateFormVersionCommandResponse>();
        response.Success = false;

        try
        {
            var apiRequest = new UpdateFormVersionApiRequest()
            {
                Data = request,
                FormVersionId = request.FormVersionId,
            };
            await _apiClient.Put(apiRequest);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
