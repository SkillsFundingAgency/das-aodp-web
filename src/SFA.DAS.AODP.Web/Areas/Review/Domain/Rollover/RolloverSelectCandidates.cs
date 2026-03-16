using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

public record RolloverSelectCandidates
{
    public SelectCandidatesForRollover? SelectedOption { get; set; }
    public string? ReturnUrl { get; set; }

    public Rollover SetSelectCandidates(Rollover session, RolloverSelectCandidatesViewModel model)
    {
        session!.SelectCandidates!.SelectedOption = model.SelectedOption;
        session.SelectCandidates.ReturnUrl = model.ReturnUrl;
        return session;
    }
}
