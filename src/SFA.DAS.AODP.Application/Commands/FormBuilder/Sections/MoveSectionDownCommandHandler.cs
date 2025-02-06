using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;

public class MoveSectionDownCommandHandler
{
    private readonly IApiClient _apiClient;

    public MoveSectionDownCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<MoveSectionDownCommandResponse>> Handle(MoveSectionDownCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<MoveSectionDownCommandResponse>()
        {
            Success = false
        };

        try
        {
            await _apiClient.Put(new MoveSectionDownApiRequest()
            {
                SectionId = request.SectionId,
                FormVersionId = request.FormVersionId
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
