namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class FundingStreamInclusionExclusionViewModel
    {
        public List<FundingStreamDto> FundingStreams { get; set; } = new();
        public List<Guid> SelectedIds { get; set; } = new();
    }
}
