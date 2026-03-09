using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public enum RolloverPreviousFileOption
{
    ContinueProcessing,
    RemovePrevious
}

public class RolloverPreviousDataViewModel
{
    public int CandidateCount { get; set; } = 0;

    [Required(ErrorMessage = "You must select what you want to do.")]
    public RolloverPreviousFileOption? SelectedOption { get; set; }
}