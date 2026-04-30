using SFA.DAS.AODP.Web.Validators.Attributes;
using SFA.DAS.AODP.Web.Validators.Patterns;

namespace SFA.DAS.AODP.Web.Models.Qualifications
{
    public class NewQualificationFilterViewModel
    {
        public NewQualificationFilterViewModel()
        {
            Organisation =string.Empty;
            QualificationName = string.Empty;
            QAN = string.Empty;
        }

        [AllowedCharacters(TextCharacterProfile.Title)]
        public string Organisation {  get; set; }

        [AllowedCharacters(TextCharacterProfile.Title)]
        public string QualificationName { get; set; }

        [AllowedCharacters(TextCharacterProfile.Title)]
        public string QAN { get; set; }

        public List<Guid>? ProcessStatusIds { get; set; } = new List<Guid>();
    }
}