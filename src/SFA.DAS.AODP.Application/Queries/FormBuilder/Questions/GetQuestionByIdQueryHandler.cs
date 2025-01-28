using MediatR;
using SFA.DAS.AODP.Domain.FormBuilder.Requests.Questions;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;

public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdQuery, BaseMediatrResponse<GetQuestionByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;


    public GetQuestionByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;

    }

    public async Task<BaseMediatrResponse<GetQuestionByIdQueryResponse>> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetQuestionByIdQueryResponse>();
        response.Success = false;
        try
        {
            var result = await _apiClient.Get<GetQuestionByIdQueryResponse>(new GetQuestionByIdApiRequest()
            {
                FormVersionId = request.FormVersionId,
                SectionId = request.SectionId,
                PageId = request.PageId,
                QuestionId = request.QuestionId
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