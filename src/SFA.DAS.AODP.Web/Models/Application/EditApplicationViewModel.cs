using SFA.DAS.AODP.Web.Validators.Attributes;
using SFA.DAS.AODP.Web.Validators.Patterns;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Application
{
    public class EditApplicationViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid ApplicationId { get; set; }

        [AllowedCharacters(TextCharacterProfile.Title)]
        [Required(ErrorMessage = "Enter a qualification title.")]
        public string Name { get; set; }

        [AllowedCharacters(TextCharacterProfile.PersonName)]
        [Required(ErrorMessage = "Enter an application owner.")]
        public string Owner { get; set; }

        [QualificationNumber]
        public string? QualificationNumber { get; set; }
    }
}