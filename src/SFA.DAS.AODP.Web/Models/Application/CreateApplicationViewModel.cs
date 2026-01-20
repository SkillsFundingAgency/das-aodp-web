using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Application
{
    public class CreateApplicationViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }

        public string FormTitle { get; set; }

        [Required(ErrorMessage = "Enter a qualification title.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Enter an application owner.")]
        public string Owner { get; set; }

        public string? QualificationNumber { get; set; }

    }
}