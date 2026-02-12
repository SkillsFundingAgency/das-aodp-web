using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public class RolloverEligibilityDatesViewModel
{
    public RolloverEligibilityDate? FundingEndDate { get; set; }

    public RolloverEligibilityDate? OperationalEndDate { get; set; }
}

public class RolloverEligibilityDate
{
    [Required]
    public int? Day { get; set; }

    [Required]
    public int? Month { get; set; }

    [Required]
    public int? Year { get; set; }

    public DateTime? ToDateTime()
    {
        if (!Day.HasValue || !Month.HasValue || !Year.HasValue)
            return null;

        try
        {
            return new DateTime(Year.Value, Month.Value, Day.Value);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}