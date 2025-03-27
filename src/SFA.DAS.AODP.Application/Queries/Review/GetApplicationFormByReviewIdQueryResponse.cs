using SFA.DAS.AODP.Domain.Application.Review;

namespace SFA.DAS.AODP.Application.Queries.Review;
public class GetApplicationFormByReviewIdQueryResponse
{
    public Guid ApplicationId { get; set; }
    public List<Question> QuestionsWithAnswers { get; set; } = new List<Question>();

    public class Question
    {
        public Guid Id { get; set; }
        public Answer? Answer { get; set; }
    }

    public class Answer
    {
        public string? TextValue { get; set; }
        public decimal? NumberValue { get; set; }
        public DateOnly? DateValue { get; set; }
        public List<string>? MultipleChoiceValue { get; set; }
        public string? RadioChoiceValue { get; set; }
    }

    public static implicit operator GetApplicationFormByReviewIdQueryResponse(GetApplicationFormAnswersByReviewIdApiResponse response)
    {
        GetApplicationFormByReviewIdQueryResponse model = new()
        {
            ApplicationId = response.ApplicationId,
        };

        foreach (var questionAnswer in response.QuestionsWithAnswers ?? [])
        {
            model.QuestionsWithAnswers.Add(new()
            {
                Id = questionAnswer.Id,
                Answer = new()
                {
                    TextValue = questionAnswer?.Answer?.TextValue,
                    DateValue = questionAnswer?.Answer?.DateValue,
                    MultipleChoiceValue = questionAnswer?.Answer?.MultipleChoiceValue,
                    NumberValue = questionAnswer?.Answer?.NumberValue,
                    RadioChoiceValue = questionAnswer?.Answer?.RadioChoiceValue
                }
            });
        }

        return model;
    }
}