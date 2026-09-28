using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Domain.Rollover;

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