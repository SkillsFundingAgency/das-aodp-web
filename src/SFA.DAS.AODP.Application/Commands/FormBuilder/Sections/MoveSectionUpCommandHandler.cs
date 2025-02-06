using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;

public class MoveSectionUpCommandHandler
{
    private readonly IApiClient _apiClient;

    public MoveSectionUpCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<MoveSectionUpCommandResponse>> Handle(MoveSectionUpCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<MoveSectionUpCommandResponse>()
        {
            Success = false
        };

        try
        {
            await _apiClient.Put(new MoveSectionUpApiRequest()
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
