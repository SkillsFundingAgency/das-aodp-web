namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class FundingStreamInclusionExclusionViewModel
    {
        public List<FundingStream> FundingStreams { get; set; } = new();
        public List<Guid> SelectedIds { get; set; } = new();
    }

    public class FundingStream
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
