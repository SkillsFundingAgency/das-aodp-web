using MediatR;
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Domain.Application.Form;
using SFA.DAS.AODP.Domain.Application.Review;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Review;

public partial class GetApplicationReadOnlyDetailsByIdQueryHandler : IRequestHandler<GetApplicationReadOnlyDetailsByIdQuery, BaseMediatrResponse<GetApplicationReadOnlyDetailsByIdQueryResponse>>
{
    private readonly IApiClient _apiClient;
    public GetApplicationReadOnlyDetailsByIdQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetApplicationReadOnlyDetailsByIdQueryResponse>> Handle(
    GetApplicationReadOnlyDetailsByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationReadOnlyDetailsByIdQueryResponse>
        {
            Success = false
        };

        try
        {
            var formPreviewResponse = await _apiClient.Get<GetFormPreviewByIdQueryResponse>(
                new GetFormPreviewByIdApiRequest(request.ApplicationId));

            var questionAnswersResponse = await _apiClient.Get<GetApplicationQuestionAnswersByIdQueryResponse>(
                new GetApplicationReadOnlyDetailsByIdApiRequest(request.ApplicationReviewId));

            var applicationDetailsResponse = new GetApplicationReadOnlyDetailsByIdQueryResponse
            {
                ApplicationId = formPreviewResponse.ApplicationId,
                SectionsWithPagesAndQuestionsAndAnswers = formPreviewResponse.SectionsWithPagesAndQuestions
                    .Select(section => new GetApplicationReadOnlyDetailsByIdQueryResponse.Section
                    {
                        Id = section.Id,
                        Order = section.Order,
                        Title = section.Title,
                        Pages = section.Pages.Select(page => new GetApplicationReadOnlyDetailsByIdQueryResponse.Page
                        {
                            Id = page.Id,
                            Order = page.Order,
                            Title = page.Title,
                            Questions = page.Questions.Select(q => new GetApplicationReadOnlyDetailsByIdQueryResponse.Question
                            {
                                Id = q.Id,
                                Title = q.Title,
                                Type = q.Type,
                                Required = q.Required,
                                QuestionAnswers = questionAnswersResponse.QuestionsWithAnswers
                                    .Where(qa => qa.Id == q.Id)
                                    .Select(qa => new GetApplicationReadOnlyDetailsByIdQueryResponse.QuestionAnswer
                                    {
                                        AnswerTextValue = qa.QuestionAnswers.FirstOrDefault()?.AnswerTextValue,
                                        AnswerDateValue = qa.QuestionAnswers.FirstOrDefault()?.AnswerDateValue,
                                        AnswerChoiceValue = qa.QuestionAnswers.FirstOrDefault()?.AnswerChoiceValue, // choice a list of choices?
                                        AnswerNumberValue = qa.QuestionAnswers.FirstOrDefault()?.AnswerNumberValue
                                    })
                                    .ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList()
            };

            response.Value = applicationDetailsResponse;
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


public class GetApplicationQuestionAnswersByIdQueryResponse
{
    public Guid ApplicationId { get; set; }
    public List<Question> QuestionsWithAnswers { get; set; } = new List<Question>();

    public class Question
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; }
        public bool Required { get; set; }
        public List<QuestionAnswer>? QuestionAnswers { get; set; } = new List<QuestionAnswer>();
    }

    public class QuestionAnswer
    {
        public string? AnswerTextValue { get; set; }
        public string? AnswerDateValue { get; set; }
        public string? AnswerChoiceValue { get; set; }
        public decimal? AnswerNumberValue { get; set; }
    }
}
