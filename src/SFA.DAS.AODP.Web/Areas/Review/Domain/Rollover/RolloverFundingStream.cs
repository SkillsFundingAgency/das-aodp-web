using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

public class RolloverFundingStream
{
    public List<FundingStream> FundingStreams { get; set; } = new();
    public List<string> SelectedIds { get; set; } = new();
}
