namespace SFA.DAS.AODP.Domain.Application.Review;

public class GetApplicationFormAnswersByReviewIdApiResponse
{
    public Guid ApplicationId { get; set; }
    public List<Question> QuestionsWithAnswers { get; set; } = new List<Question>();

    public class Question
    {
        public Guid Id { get; set; }
        public Answer? Answer { get; set; } = new();
    }

    public class Answer
    {
        public string? TextValue { get; set; }
        public decimal? NumberValue { get; set; }
        public DateOnly? DateValue { get; set; }
        public List<string>? MultipleChoiceValue { get; set; }
        public string? RadioChoiceValue { get; set; }
    }
}