using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;

namespace SFA.DAS.AODP.Web.Models.Section
{
    public class EditSectionViewModel
    {
        public int Order { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }


        public static EditSectionViewModel Map(GetSectionByIdQueryResponse source)
        {
            return new()
            {
                Description = source.Data.Description,
                Order = source.Data.Order,
                Title = source.Data.Title,
                FormVersionId = source.Data.FormVersionId,
                SectionId = source.Data.Id
            };
        }
    }
}
