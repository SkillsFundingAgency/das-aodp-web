using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Questions;
using SFA.DAS.AODP.Domain.FormBuilder.Responses.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Responses.Questions;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;

public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdQuery, GetQuestionByIdQueryResponse>
{
    private readonly IApiClient _apiClient;


    public GetQuestionByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<GetQuestionByIdQueryResponse> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new GetQuestionByIdQueryResponse();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetQuestionByIdApiResponse>(new GetQuestionByIdApiRequest()
            {
                FormVersionId = request.FormVersionId,
                SectionId = request.SectionId,
                PageId = request.PageId,
                QuestionId = request.QuestionId
            });
            response.Data = result.Data;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}