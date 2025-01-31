using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Application
{
    public class CreateApplicationViewModel
    {
        public Guid OrganisationId { get; set; }
        [Required]
        public Guid FormVersionId { get; set; }
        [Required]
        public string Name { get; set; }

    }
}