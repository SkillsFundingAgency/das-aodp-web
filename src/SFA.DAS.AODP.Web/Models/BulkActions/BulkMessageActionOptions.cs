using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.AODP.Application.Commands.Review;
using SFA.DAS.AODP.Models.Users;

namespace SFA.DAS.AODP.Web.Models.BulkActions.Options;

public static class BulkMessageActionOptions
{
    public static List<SelectListItem> Build() =>
        Enum.GetValues<BulkApplicationActionType>()
            .Select(x => new SelectListItem
            {
                Value = x.ToString(),
                Text = GetDisplayText(x)
            })
            .Prepend(new SelectListItem { Value = "", Text = "Choose action" })
            .ToList();

    private static string GetDisplayText(BulkApplicationActionType action) =>
        action switch
        {
            BulkApplicationActionType.ShareWithSkillsEngland => "Share with Skills England",
            BulkApplicationActionType.ShareWithOfqual => "Share with Ofqual",
            BulkApplicationActionType.Unlock => "Unlock application",
            _ => action.ToString()
        };
}
