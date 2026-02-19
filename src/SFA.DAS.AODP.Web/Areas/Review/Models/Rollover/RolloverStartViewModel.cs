using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public enum RolloverProcess
{
    InitialSelection,
    FinalUpload
}

public class RolloverStartViewModel
{
    [Required(ErrorMessage = "You must select which stage of the rollover process you need to do.")]
    public RolloverProcess? SelectedProcess { get; set; }
}
