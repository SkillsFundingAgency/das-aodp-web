using SFA.DAS.AODP.Web.Validators.Attributes;
using SFA.DAS.AODP.Web.Validators.Patterns;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Form;

public class CreateFormVersionViewModel
{
    [AllowedCharacters(TextCharacterProfile.Title)]
    public string Name { get; set; }

    [AllowedCharacters(TextCharacterProfile.FreeText)]
    public string Description { get; set; }

    public string? DescriptionPreview { get; set; }
    public AdditionalActions AdditionalFormActions { get; set; } = new AdditionalActions();

    public class AdditionalActions
    {
        public bool UpdateDescriptionPreview { get; set; }
    }
}
