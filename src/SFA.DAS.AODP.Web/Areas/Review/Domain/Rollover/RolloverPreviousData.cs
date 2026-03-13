using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

public record RolloverPreviousData
{
    public int CandidateCount { get; set; }
    public RolloverPreviousFileOption? SelectedOption { get; set; }

    public Rollover SetPreviousDataCandidate(Rollover session, RolloverPreviousDataViewModel previousData)
    {
        session!.PreviousData!.CandidateCount = previousData.CandidateCount;
        session!.PreviousData!.SelectedOption = previousData.SelectedOption;
        return session;
    }
}