using SFA.DAS.AODP.Application.Queries.Review;
using SFA.DAS.AODP.Infrastructure.File;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;

public class ApplicationReadOnlyDetailsViewModel
{
    public Guid ApplicationReviewId { get; set; }
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
        public string? FinalAnswer { get; set; }
    }

    public static ApplicationReadOnlyDetailsViewModel Map(GetApplicationReadOnlyDetailsByIdQueryResponse response)
    {
        return new ApplicationReadOnlyDetailsViewModel
        {
            ApplicationId = response.ApplicationId,
            SectionsWithPagesAndQuestionsAndAnswers = response.SectionsWithPagesAndQuestionsAndAnswers
                .Select(section => new Section
                {
                    Id = section.Id,
                    Order = section.Order,
                    Title = section.Title,
                    Pages = section.Pages.Select(page => new Page
                    {
                        Id = page.Id,
                        Order = page.Order,
                        Title = page.Title,
                        Questions = page.Questions.Select(question => new Question
                        {
                            Id = question.Id,
                            Title = question.Title,
                            Type = question.Type,
                            Required = question.Required,
                            QuestionOptions = question.QuestionOptions.Select(option => new QuestionOption
                            {
                                Value = option.Value,
                                Selected = option.Selected
                            }).ToList(),
                            QuestionAnswers = question.QuestionAnswers.Select(answer => new QuestionAnswer
                            {
                                FinalAnswer = AnswerSelector.GetReadOnlyAnswer(answer, question.Type)
                            }).ToList(),
                            Files = question.Files
                        }).ToList()
                    }).ToList()
                }).ToList()
        };
    }
}