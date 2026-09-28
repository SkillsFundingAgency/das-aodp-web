using SFA.DAS.AODP.Application.Commands.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class FundingStreamInclusionExclusionViewModel
    {
        public List<FundingStreamDto> FundingStreams { get; set; } = new();

        public List<Guid> SelectedIds { get; set; } = new();

        public SelectCandidatesForRollover? SelectionMethod { get; set; }
    }
}