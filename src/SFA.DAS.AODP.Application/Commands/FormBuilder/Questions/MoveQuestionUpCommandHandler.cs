using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Questions;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;
using SFA.DAS.AODP.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Questions;

public class MoveQuestionUpCommandHandler
{
    private readonly IApiClient _apiClient;

    public MoveQuestionUpCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<MoveQuestionUpCommandResponse>> Handle(MoveQuestionUpCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<MoveQuestionUpCommandResponse>()
        {
            Success = false
        };

        try
        {
            await _apiClient.Put(new MoveQuestionUpApiRequest()
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
