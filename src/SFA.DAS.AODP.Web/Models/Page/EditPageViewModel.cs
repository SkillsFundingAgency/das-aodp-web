using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Domain.FormBuilder.Responses.Pages;

namespace SFA.DAS.AODP.Web.Models.Page
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
                Description = source.Data.Description,
                Order = source.Data.Order,
                Title = source.Data.Title,
                FormVersionId = formVersionId,
                SectionId = source.Data.SectionId,
                PageId = source.Data.Id,
                Questions = source.Data.Questions != null ? [.. source.Data.Questions] : new()
            };
        }
    }
}
