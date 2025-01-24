using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;

namespace SFA.DAS.AODP.Web.Models.Section
{
    public class EditPageViewModel
    {
        public int Order { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public Guid PageId { get; set; }



        public static EditPageViewModel Map(GetPageByIdQueryResponse source, Guid formVersionId)
        {
            return new()
            {
                Description = source.Data.Description,
                Order = source.Data.Order,
                Title = source.Data.Title,
                FormVersionId = formVersionId,
                SectionId = source.Data.SectionId,
                PageId = source.Data.Id
            };
        }
    }
}
