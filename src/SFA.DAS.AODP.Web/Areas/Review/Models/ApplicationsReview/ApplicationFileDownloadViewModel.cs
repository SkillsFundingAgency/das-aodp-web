using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;

public class ApplicationFileDownloadViewModel
{
    public Guid ApplicationReviewId { get; set; }
    public string FilePath { get; set; }
    [Required]
    public Guid FileId { get; set; }
}
