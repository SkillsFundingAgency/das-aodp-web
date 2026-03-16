using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

[ExcludeFromCodeCoverage]
public record Rollover
{
    public RolloverStart? Start { get; set; }
    public RolloverImportStatus? ImportStatus { get; set; }
    public RolloverPreviousData? PreviousData { get; set; }
    public RolloverSelectCandidates? SelectCandidates { get; set; }
}
