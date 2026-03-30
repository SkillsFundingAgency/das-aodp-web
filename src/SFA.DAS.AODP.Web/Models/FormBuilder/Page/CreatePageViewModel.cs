using SFA.DAS.AODP.Web.Validators.Attributes;
using SFA.DAS.AODP.Web.Validators.Patterns;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Page
{
    public class CreatePageViewModel
    {
        [AllowedCharacters(TextCharacterProfile.Title)]
        public string Title { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
    }
}
