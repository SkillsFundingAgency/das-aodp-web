using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class RolloverUploadQualificationsViewModel
    {
        [Required(ErrorMessage = "You must select a CSV file.")]
        public IFormFile File { get; set; }
    }
}