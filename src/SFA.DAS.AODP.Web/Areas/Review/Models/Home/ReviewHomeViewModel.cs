using SFA.DAS.AODP.Models.Users;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Home
{
    public class ReviewHomeViewModel
    {
        public enum Options
        {
            NewQualifications, ChangedQualifications, ImportData, OutputFile,

            ApplicationsReview, FormsManagement,
        }

        [DisplayName("Option")]
        [Required]
        public Options? SelectedOption { get; set; }

        public List<string> UserRoles { get; set; } = new();
    }
}
