using SFA.DAS.AODP.Application.Queries.Import;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Import
{
    public class ImportRequestViewModel
    {
        [Required]
        public string ImportType { get; set; } = string.Empty;       
    }
}
