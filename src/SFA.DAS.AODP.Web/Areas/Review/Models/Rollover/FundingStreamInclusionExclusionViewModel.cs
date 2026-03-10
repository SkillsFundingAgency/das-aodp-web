namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class FundingStreamInclusionExclusionViewModel
    {
        public List<FundingStream> FundingStreams { get; set; } = new();
        public List<string> SelectedIds { get; set; } = new();
    }

    public class FundingStream
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
