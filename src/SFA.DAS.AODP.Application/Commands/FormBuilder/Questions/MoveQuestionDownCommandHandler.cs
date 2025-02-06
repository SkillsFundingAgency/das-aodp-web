using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Questions;
using SFA.DAS.AODP.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;

public class MoveQuestionDownCommandHandler
{
    private readonly IApiClient _apiClient;

    public MoveQuestionDownCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<MoveQuestionDownCommandResponse>> Handle(MoveQuestionDownCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<MoveQuestionDownCommandResponse>()
        {
            Success = false
        };

        try
        {
            await _apiClient.Put(new MoveQuestionDownApiRequest()
            {
                QuestionId = request.QuestionId,
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
