using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Page
{
    public class EditPageViewModel
    {
        public int Order { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public Guid PageId { get; set; }
        public List<Question> Questions { get; set; }

        public AdditionalPageActions AdditionalFormActions { get; set; } = new AdditionalPageActions();

        public class AdditionalPageActions
        {
            public Guid? MoveUp { get; set; }
            public Guid? MoveDown { get; set; }

        }


        public class Question
        {
            public Guid Id { get; set; }
            public Guid Key { get; set; }
            public int Order { get; set; }
            public string Title { get; set; }

            public static implicit operator Question(GetPageByIdQueryResponse.Question question)
            {
                return new()
                {
                    Id = question.Id,
                    Key = question.Key,
                    Order = question.Order,
                    Title = question.Title,
                };
            }
        }

        public static EditPageViewModel Map(GetPageByIdQueryResponse source, Guid formVersionId)
        {
            return new()
            {
                Description = source.Description,
                Order = source.Order,
                Title = source.Title,
                FormVersionId = formVersionId,
                SectionId = source.SectionId,
                PageId = source.Id,
                Questions = source.Questions != null ? [.. source.Questions] : new()
            };
        }
    }
}
