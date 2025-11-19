using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Import.Models;

public class UploadDefundingListViewModel
{
    [Required(ErrorMessage = "You must select an .xlsx file")]
    [Display(Name = "You must select an .xlsx file")]
    public IFormFile File { get; set; }

    //public FormBuilderSettings FileSettings { get; set; }
}