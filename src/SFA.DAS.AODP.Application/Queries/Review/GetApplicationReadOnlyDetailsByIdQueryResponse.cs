﻿using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Forms;
using static SFA.DAS.AODP.Application.Queries.Review.GetApplicationReadOnlyDetailsByIdQueryResponse;

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
        public List<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();
        public List<QuestionAnswer> QuestionAnswers { get; set; } = new List<QuestionAnswer>();
        public List<UploadedBlob>? Files { get; set; }
    }

    public class QuestionOption
    {
        public string Value { get; set; }
        public bool Selected { get; set; }
    }

    public class QuestionAnswer
    {
        public string? AnswerTextValue { get; set; }
        public string? AnswerDateValue { get; set; }
        public string? AnswerChoiceValue { get; set; }
        public decimal? AnswerNumberValue { get; set; }
    }
}


public static class AnswerSelector
{
    private static readonly Dictionary<QuestionType, Func<QuestionAnswer, string>> _answerSelectors =
        new()
        {
            { QuestionType.Text, answer => answer.AnswerTextValue ?? "" },
            { QuestionType.TextArea, answer => answer.AnswerTextValue ?? "" },
            { QuestionType.Number, answer => answer.AnswerNumberValue.HasValue ? Math.Floor(answer.AnswerNumberValue.Value).ToString() : "" },
            { QuestionType.Date, answer => answer.AnswerDateValue != null ? DateTime.Parse(answer.AnswerDateValue).ToString("dd MMM yyyy"): "" },
            { QuestionType.MultiChoice, answer => answer.AnswerTextValue ?? "" },
            { QuestionType.Radio, answer => answer.AnswerChoiceValue ?? "" }
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