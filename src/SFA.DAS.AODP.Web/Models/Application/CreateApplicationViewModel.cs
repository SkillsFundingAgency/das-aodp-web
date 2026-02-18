using Microsoft.AspNetCore.Components.Forms;
using SFA.DAS.AODP.Web.Validators.Attributes;
using SFA.DAS.AODP.Web.Validators.Messages;
using SFA.DAS.AODP.Web.Validators.Patterns;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Application
{
    public class CreateApplicationViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }
        public string FormTitle { get; set; }

        [Required(ErrorMessage = ValidationMessages.QualificationTitleRequired)]
        [StringLength(200, ErrorMessage = ValidationMessages.QualificationTitleTooLong)]
        [AllowedCharacters(TextCharacterProfile.QualificationTitle, 
            ErrorMessage = ValidationMessages.QualificationTitleInvalidChars)]
        public string Name { get; set; }

        [Required(ErrorMessage = ValidationMessages.OwnerRequired)]
        [StringLength(200, ErrorMessage = ValidationMessages.OwnerTooLong)]
        [AllowedCharacters(TextCharacterProfile.PersonName, 
            ErrorMessage = ValidationMessages.OwnerInvalidChars)]
        public string Owner { get; set; }

        [QualificationNumber]
        public string? QualificationNumber { get; set; }

    }
}