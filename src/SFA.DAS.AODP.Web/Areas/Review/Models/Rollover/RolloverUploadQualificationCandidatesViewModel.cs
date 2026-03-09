using SFA.DAS.AODP.Application.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class RolloverUploadQualificationCandidatesViewModel
    {
        [Required(ErrorMessage = "You must select a CSV file.")]
        public IFormFile File { get; set; }
    }
}