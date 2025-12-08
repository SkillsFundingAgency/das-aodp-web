using SFA.DAS.AODP.Models.Users;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Home
{
    public class ReviewHomeViewModel
    {
        public List<string> UserRoles { get; set; } = new();
    }
}
