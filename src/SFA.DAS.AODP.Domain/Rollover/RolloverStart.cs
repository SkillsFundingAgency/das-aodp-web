using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Domain.Rollover;

public record RolloverStart
{
    public RolloverProcess? SelectedProcess { get; set; }

    public Rollover SetStart(Rollover session, RolloverProcess? selectedProcess)
    {
        session!.Start!.SelectedProcess = selectedProcess;
        return session;
    }
}