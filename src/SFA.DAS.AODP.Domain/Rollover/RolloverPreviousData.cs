namespace SFA.DAS.AODP.Domain.Rollover;

public record RolloverPreviousData
{
    public int CandidateCount { get; set; }
    public RolloverPreviousFileOption? SelectedOption { get; set; }

    public Rollover SetPreviousDataCandidate(Rollover session, int candidateCount, RolloverPreviousFileOption? selectedOption)
    {
        session!.PreviousData!.CandidateCount = candidateCount;
        session!.PreviousData!.SelectedOption = selectedOption;
        return session;
    }
}