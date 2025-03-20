using MediatR;
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Domain.Application.Form;
using SFA.DAS.AODP.Domain.Application.Review;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Models.Forms;

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
                            Questions = page.Questions.Select(q =>
                            {
                                var matchingAnswers = questionAnswersResponse.QuestionsWithAnswers
                                    .Where(qa => qa.Id == q.Id)
                                    .ToList();

                                return new GetApplicationReadOnlyDetailsByIdQueryResponse.Question
                                {
                                    Id = q.Id,
                                    Title = q.Title,
                                    Type = q.Type,
                                    Required = q.Required,
                                    QuestionAnswers = ExtractQuestionAnswers(matchingAnswers),
                                    QuestionOptions = ExtractQuestionOptions(q, matchingAnswers)
                                };
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

    private List<GetApplicationReadOnlyDetailsByIdQueryResponse.QuestionAnswer> ExtractQuestionAnswers(
       List<GetApplicationQuestionAnswersByIdQueryResponse.Question> matchingAnswers)
    {
        return matchingAnswers.SelectMany(qa => qa.QuestionAnswers ?? new List<GetApplicationQuestionAnswersByIdQueryResponse.QuestionAnswer>())
            .Select(qa => new GetApplicationReadOnlyDetailsByIdQueryResponse.QuestionAnswer
            {
                AnswerTextValue = qa.AnswerTextValue,
                AnswerDateValue = qa.AnswerDateValue,
                AnswerChoiceValue = qa.AnswerChoiceValue,
                AnswerNumberValue = qa.AnswerNumberValue
            }).ToList();
    }

    private List<GetApplicationReadOnlyDetailsByIdQueryResponse.QuestionOption> ExtractQuestionOptions(
        GetFormPreviewByIdQueryResponse.Question q,
        List<GetApplicationQuestionAnswersByIdQueryResponse.Question> matchingAnswers)
    {
        return q.QuestionOptions.Select(option =>
        {
            var optionId = option.Id;
            var selected = false;

            foreach (var qa in matchingAnswers.SelectMany(qa => qa.QuestionAnswers ?? new List<GetApplicationQuestionAnswersByIdQueryResponse.QuestionAnswer>()))
            {
                var answerValue = qa.AnswerChoiceValue;

                if (!string.IsNullOrEmpty(answerValue))
                {
                    if (q.Type == QuestionType.MultiChoice.ToString())
                    {
                        var selectedGuids = answerValue
                            .Split(',')
                            .Select(value => Guid.TryParse(value.Trim(), out var parsedGuid) ? parsedGuid : (Guid?)null)
                            .Where(guid => guid.HasValue)
                            .Select(guid => guid.Value)
                            .ToList();

                        if (selectedGuids.Contains(optionId))
                        {
                            selected = true;
                            break;
                        }
                    }
                    else
                    {
                        if (Guid.TryParse(answerValue, out var answerGuid) && answerGuid == optionId)
                        {
                            selected = true;
                            break;
                        }
                    }
                }
            }

            return new GetApplicationReadOnlyDetailsByIdQueryResponse.QuestionOption
            {
                Value = option.Value,
                Selected = selected
            };
        }).ToList();
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
