﻿using MediatR;
using SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class MovePageDownCommandHandler : IRequestHandler<MovePageDownCommand, BaseMediatrResponse<MovePageDownCommandResponse>>
{
    private readonly IApiClient _apiClient;

    public MovePageDownCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<MovePageDownCommandResponse>> Handle(MovePageDownCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<MovePageDownCommandResponse>()
        {
            Success = false
        };

        try
        {
            var apiRequest = new MovePageDownApiRequest()
            {
                PageId = request.PageId,
                FormVersionId = request.FormVersionId,
                SectionId = request.SectionId,
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
