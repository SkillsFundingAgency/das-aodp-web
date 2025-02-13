using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class MovePageUpCommandHandler : IRequestHandler<MovePageUpCommand, BaseMediatrResponse<MovePageUpCommandResponse>>
{
    private readonly IApiClient _apiClient;

    public MovePageUpCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<MovePageUpCommandResponse>> Handle(MovePageUpCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<MovePageUpCommandResponse>()
        {
            Success = false
        };

        try
        {
            await _apiClient.Put(new MovePageUpApiRequest()
            {
                FormVersionId = request.FormVersionId,
                PageId = request.PageId,
                SectionId = request.SectionId
            });
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
