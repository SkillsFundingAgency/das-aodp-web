using SFA.DAS.AODP.Web.Views.Shared;

namespace SFA.DAS.AODP.Web.Factories;

public static class BackLinkModelFactory
{
    public static BackLinkModel Dashboard(string text = "Back")
    {
        return new BackLinkModel
        {
            Controller = "Home",
            Action = "Index",
            Text = text
        };
    }
}