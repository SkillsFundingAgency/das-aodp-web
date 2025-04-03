using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Application.Queries.Review;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Forms;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;

public class ApplicationReadOnlyDetailsViewModel
{
    public Guid ApplicationReviewId { get; set; }
    public Guid ApplicationId { get; set; }
    public List<Section> Sections { get; set; } = new List<Section>();

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
        public int Order { get; set; }
        public List<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();
        public Answer? Answer { get; set; } = new Answer();
    }

    public class QuestionOption
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }
    }

    public class Answer
    {
        public string? TextValue { get; set; }
        public decimal? NumberValue { get; set; }
        public DateOnly? DateValue { get; set; }
        public List<string>? MultipleChoiceValue { get; set; }
        public string? RadioChoiceValue { get; set; }
        public List<File>? Files { get; set; }

    }

    public class File
    {
        public string FileDisplayName { get; set; }
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
    }

    public static ApplicationReadOnlyDetailsViewModel Map(GetFormPreviewByIdQueryResponse form, GetApplicationFormByReviewIdQueryResponse answers, List<UploadedBlob> files)
    {
        var model = new ApplicationReadOnlyDetailsViewModel
        {
            ApplicationId = answers.ApplicationId,
            Sections = form.SectionsWithPagesAndQuestions
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
                            Order = question.Order,
                            QuestionOptions = question.QuestionOptions.Select(option => new QuestionOption
                            {
                                Id = option.Id,
                                Order = option.Order,
                                Value = option.Value,
                            }).ToList(),
                            Answer = GetAnswer(question, answers, files)
                        }).ToList()
                    }).ToList()
                }).ToList()
        };

        return model;
    }

    private static Answer? GetAnswer(GetFormPreviewByIdQueryResponse.Question question, GetApplicationFormByReviewIdQueryResponse answers, List<UploadedBlob> files)
    {
        if (question.Type == QuestionType.File.ToString()) return new Answer()
        {
            Files = files.Where(f => f.FullPath.StartsWith($"{answers.ApplicationId}/{question.Id}")).ToList().Select(s => new File()
            {
                Extension = s.Extension,
                FileDisplayName = s.FileNameWithPrefix,
                FullPath = s.FullPath,
                FileName = s.FileName,
            }).ToList()
        };

        var answer = answers.QuestionsWithAnswers.FirstOrDefault(q => q.Id == question.Id)?.Answer;
        if (answer == null) return null;

        return new()
        {
            DateValue = answer.DateValue,
            TextValue = answer.TextValue,
            NumberValue = answer.NumberValue,
            MultipleChoiceValue = answer.MultipleChoiceValue,
            RadioChoiceValue = answer.RadioChoiceValue
        };
    }

    public bool HasAnyFiles()
    {
        if (Sections == null) return false;

        return Sections
            .SelectMany(s => s.Pages ?? Enumerable.Empty<Page>()) 
            .SelectMany(p => p.Questions ?? Enumerable.Empty<Question>()) 
            .Any(q => q.Answer?.Files != null && q.Answer.Files.Any()); 
    }
}