using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Application
{
    public class EditApplicationViewModel
    {
        public Guid OrganisationId { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid ApplicationId { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Owner { get; set; }

        public string? QualificationNumber { get; set; }

    }
}