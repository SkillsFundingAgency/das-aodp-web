using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public enum IncludeHoldListOption
{
    Yes,
    No
}

public class RolloverIncludeHoldListViewModel
{
    [Required(ErrorMessage = "You must select whether to include the hold list.")]
    public IncludeHoldListOption? SelectedOption { get; set; }
}