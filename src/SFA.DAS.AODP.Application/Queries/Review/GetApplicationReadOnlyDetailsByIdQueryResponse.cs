using SFA.DAS.AODP.Models.Forms;

namespace SFA.DAS.AODP.Application.Queries.Review;

public class GetApplicationReadOnlyDetailsByIdQueryResponse
{
    public Guid ApplicationId { get; set; }
    public List<Section> SectionsWithPagesAndQuestionsAndAnswers { get; set; } = new List<Section>();

    public class Section
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
        public string Title { get; set; }
        public List<Page> Pages { get; set; } = new List<Page>();
    }

    public class Page
    {
        public Guid Id { get; set; }
        public int Order { get; set; }
        public string Title { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
    }

    public class Question
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; }
        public bool Required { get; set; }
        public List<QuestionAnswer> QuestionAnswers { get; set; } = new List<QuestionAnswer>();
    }

    public class QuestionAnswer
    {
        public string? AnswerTextValue { get; set; }
        public string? AnswerDateValue { get; set; }
        public string? AnswerChoiceValue { get; set; }
        public decimal? AnswerNumberValue { get; set; }
    }

    public static class AnswerSelector
    {
        private static readonly Dictionary<QuestionType, Func<QuestionAnswer, string>> _answerSelectors =
            new()
            {
            { QuestionType.Text, answer => answer.AnswerTextValue ?? "No Answer" },
            { QuestionType.TextArea, answer => answer.AnswerTextValue ?? "No Answer" },
            { QuestionType.Number, answer => answer.AnswerNumberValue?.ToString() ?? "No Answer" },
            { QuestionType.Date, answer => answer.AnswerDateValue ?? "No Answer" },
            { QuestionType.MultiChoice, answer => answer.AnswerTextValue ?? "No Answer" },
            { QuestionType.Radio, answer => answer.AnswerChoiceValue ?? "No Answer" },
            { QuestionType.File, answer => "File TODO" }
            };

        public static string GetReadOnlyAnswer(QuestionAnswer answer, string questionType)
        {
            if (!Enum.TryParse(questionType, out QuestionType type) || !_answerSelectors.ContainsKey(type))
            {
                return "Invalid Question Type";
            }

            return _answerSelectors[type](answer);
        }
    }
}
