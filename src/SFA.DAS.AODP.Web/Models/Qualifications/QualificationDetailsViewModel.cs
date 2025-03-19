using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Qualifications
{
    public class QualificationDetailsViewModel
    {
        public Guid Id { get; set; }
        public string? Status { get; set; }
        [Required]
        public Guid? StatusId { get; set; }
        public string? Priority { get; set; }
        public string? Changes { get; set; }
        public string? QualificationReference { get; set; }
        public string? AwardingOrganisation { get; set; }
        public string? Title { get; set; }
        public string? QualificationType { get; set; }
        public string? Level { get; set; }
        public string? ProposedChanges { get; set; }
        public string? AgeGroup { get; set; }
        public string? Category { get; set; }
        public string? Subject { get; set; }
        public string? SectorSubjectArea { get; set; }
        public string? Comments { get; set; }

        [Required(AllowEmptyStrings = false)]
        public List<SelectListItem>? ActionTypes { get; set; }
    }
}

