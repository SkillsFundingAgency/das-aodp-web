using System.Collections;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

[ExcludeFromCodeCoverage]
public record CheckYourAnswersViewModel
{
    public List<QualificationLevel> Levels { get; set; } = [];

    public List<QualificationType> Types { get; set; } = [];
}