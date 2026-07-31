namespace SFA.DAS.AODP.Domain.Rollover;

public record RolloverSelectCandidates
{
    public SelectCandidatesForRollover? SelectedOption { get; set; }
    public string? ReturnUrl { get; set; }

    public Rollover SetSelectCandidates(Rollover session, SelectCandidatesForRollover? selectedOption, string? returnUrl)
    {
        session!.SelectCandidates!.SelectedOption = selectedOption;
        session.SelectCandidates.ReturnUrl = returnUrl;
        return session;
    }
}
