using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Import.Models;

public class UploadDefundingListViewModel
{
    [Required(ErrorMessage = "You must select an .xlsx file")]
    [Display(Name = "You must select an .xlsx file")]
    public required IFormFile File { get; set; }
}