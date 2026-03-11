using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public enum SelectCandidatesForRollover
{
    ImportAList,
    GenerateAList
}

public class RolloverSelectCandidatesViewModel
{
    [Required(ErrorMessage = "You must select an option")]
    public SelectCandidatesForRollover? SelectedOption { get; set; }

    public string? ReturnUrl { get; set; }
}
