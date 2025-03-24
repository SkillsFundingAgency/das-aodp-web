using GovUk.Frontend.AspNetCore;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Web.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SFA.DAS.AODP.Web.Models.Import
{
    public class SubmitImportRequestViewModel
    {
        [Required]
        public string ImportType { get; set; } = string.Empty;

        [DateInput]
        public DateTime SubmittedTime { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string StatusMessage
        {
            get {
                var joinWord = "at";                
                if (Status == JobStatus.Running.ToString())
                {
                    joinWord = "since";
                }
                
                return $"{Status} {joinWord} {SubmittedTime.ToShortDateString()} {SubmittedTime.ToShortTimeString()} by {UserName}";
            }
        }
        public string JobName { get; set; } = string.Empty;

        public Guid JobRunId { get; set; } = Guid.Empty;
    }
}
