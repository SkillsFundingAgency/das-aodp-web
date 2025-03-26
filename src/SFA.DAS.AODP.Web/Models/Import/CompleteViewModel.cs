using GovUk.Frontend.AspNetCore;
using SFA.DAS.AODP.Application.Queries.Import;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Import
{
    public class CompleteViewModel
    {
        public Guid JobRunId { get; set; } = Guid.Empty;

        [Required]
        public string ImportType { get; set; } = string.Empty;

        [DateInput]
        public DateTime CompletedTime { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string JobName { get; set; } = string.Empty;
    }
}
