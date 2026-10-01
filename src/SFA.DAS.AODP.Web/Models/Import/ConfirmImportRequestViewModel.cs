using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Import
{
    [ExcludeFromCodeCoverage]
    public class ConfirmImportRequestViewModel
    {
        [Required]
        public string ImportType { get; set; } = string.Empty;       
    }
}
