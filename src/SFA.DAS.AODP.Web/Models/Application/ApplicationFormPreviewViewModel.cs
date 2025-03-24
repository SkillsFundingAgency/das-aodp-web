using SFA.DAS.AODP.Application.Queries.Application.Form;

namespace SFA.DAS.AODP.Web.Models.Application;

public class ApplicationFormPreviewViewModel
{
    public Guid OrganisationId { get; set; }
    public Guid FormVersionId { get; set; }
    public Guid ApplicationId { get; set; }
    public string? FormTitle { get; set; }

    public List<SectionViewModel> Sections { get; set; } = new List<SectionViewModel>();

    public static ApplicationFormPreviewViewModel Map(GetFormPreviewByIdQueryResponse formPreviewResponse,
        Guid formVersionId,
        Guid organisationId,
        Guid applicationId)
    {
        return new ApplicationFormPreviewViewModel
        {
            ApplicationId = applicationId,
            OrganisationId = organisationId,
            FormVersionId = formVersionId,
            Sections = formPreviewResponse.SectionsWithPagesAndQuestions.Select(s => new SectionViewModel
            {
                Id = s.Id,
                Title = s.Title,
                Order = s.Order,
                TotalPages = s.Pages.Count,
                Pages = s.Pages.Select(p => new PageViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Order = p.Order,
                    Questions = p.Questions.Select(q => new QuestionViewModel
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Type = q.Type,
                        Required = q.Required,
                        Order = q.Order,
                        QuestionOptions = q.QuestionOptions.Select(opt => new QuestionOptionViewModel
                        {
                            Value = opt.Value,
                            Order = opt.Order,
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList()
        };
    }

    public class SectionViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public int TotalPages { get; set; }

        public List<PageViewModel> Pages { get; set; } = new List<PageViewModel>();
    }

    public class PageViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
    }

    public class QuestionViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; }
        public bool Required { get; set; }
        public int Order { get; set; }
        public List<QuestionOptionViewModel> QuestionOptions { get; set; } = new List<QuestionOptionViewModel>();
    }

    public class QuestionOptionViewModel
    {
        public string Value { get; set; }
        public int Order { get; set; }
    }
}
