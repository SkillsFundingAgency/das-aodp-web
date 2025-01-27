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
        public List<Page> Pages { get; set; }
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
                Description = source.Data.Description,
                Order = source.Data.Order,
                Title = source.Data.Title,
                FormVersionId = source.Data.FormVersionId,
                SectionId = source.Data.Id,
                Pages = source.Data.Pages != null ? [..source.Data.Pages] : new()
            };
        }
    }
}
