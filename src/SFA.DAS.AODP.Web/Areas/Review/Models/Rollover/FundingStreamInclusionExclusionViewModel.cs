namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class FundingStreamInclusionExclusionViewModel
    {
        public List<FundingStream> FundingStreams { get; set; } = new();
        public List<int> SelectedIds { get; set; } = new();
    }

    public class FundingStream
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
