using SFA.DAS.AODP.Web.Validators.Attributes;
using SFA.DAS.AODP.Web.Validators.Patterns;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Section
{
    public class CreateSectionViewModel
    {
        [AllowedCharacters(TextCharacterProfile.Title)]
        public string Title { get; set; }
        public Guid FormVersionId { get; set; }
    }
}
