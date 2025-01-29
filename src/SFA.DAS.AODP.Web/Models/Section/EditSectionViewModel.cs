using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SFA.DAS.AODP.Web.Models.Section
{
    public class EditSectionViewModel
    {
        public int Order { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public List<Page> Pages { get; set; } = new();
        public class Page
        {
            public Guid Id { get; set; }
            public Guid Key { get; set; }
            public int Order { get; set; }
            public string Title { get; set; }

            public static implicit operator Page(GetSectionByIdQueryResponse.Page entity)
            {
                return new()
                {
                    Id = entity.Id,
                    Key = entity.Key,
                    Order = entity.Order,
                    Title = entity.Title
                };
            }
        }


        public static EditSectionViewModel Map(GetSectionByIdQueryResponse source)
        {
            return new()
            {
                Description = source.Description,
                Order = source.Order,
                Title = source.Title,
                FormVersionId = source.FormVersionId,
                SectionId = source.Id,
                Pages = source.Pages != null ? [..source.Pages] : new()
            };
        }
    }
}
