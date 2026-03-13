using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

public record RolloverStart
{
    public RolloverProcess? SelectedProcess { get; set; }

    public Rollover SetStart(Rollover session, RolloverStartViewModel model)
    {
        session!.Start!.SelectedProcess = model.SelectedProcess;
        return session;
    }
}