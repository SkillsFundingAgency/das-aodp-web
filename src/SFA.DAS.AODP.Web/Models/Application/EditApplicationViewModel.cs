using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Application
{
    public class EditApplicationViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid ApplicationId { get; set; }

        [Required(ErrorMessage = "Enter a qualification title.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Enter an application owner.")]
        public string Owner { get; set; }

        public string? QualificationNumber { get; set; }

    }
}