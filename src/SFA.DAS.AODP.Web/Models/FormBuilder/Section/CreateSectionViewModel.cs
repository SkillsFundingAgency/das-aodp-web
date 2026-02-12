using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Section
{
    public class CreateSectionViewModel
    {
        [Required]
        public string Title { get; set; }
        public Guid FormVersionId { get; set; }

    }
}
