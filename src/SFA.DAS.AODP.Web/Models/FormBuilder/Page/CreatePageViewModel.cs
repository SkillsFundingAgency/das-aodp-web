using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Page
{
    public class CreatePageViewModel
    {
        [Required]
        public string Title { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }

    }
}
