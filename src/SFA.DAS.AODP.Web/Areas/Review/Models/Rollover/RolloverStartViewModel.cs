using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

[ExcludeFromCodeCoverage]
public class RolloverStartViewModel
{
    [Required(ErrorMessage = "You must select which stage of the rollover process you need to do.")]
    public RolloverProcess? SelectedProcess { get; set; }
    public int TotalCandidatesCount { get; set; }
    public int CandidatesEligibleCount { get; set; }
    public int CandidatesIneligibleCount { get; set; }
    public int CandidatesRemainingCount { get; set; }

}
