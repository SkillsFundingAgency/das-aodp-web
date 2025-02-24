using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SFA.DAS.AODP.Web.Helpers.ListHelper
{
    public static class OrderButtonHelper
    {
        public enum UpdateTempDataKeys { FocusItemId, Directon }
        public enum OrderDirection { Up, Down }

        public static bool ShouldAutoFocus(ITempDataDictionary tempData, OrderDirection buttonDirection, Guid itemId, int index, int totalItems)
        {
            var focus = false;
            if (buttonDirection == OrderDirection.Up && index != totalItems - 1)
            {
                focus = tempData[UpdateTempDataKeys.Directon.ToString()]?.ToString() == OrderDirection.Up.ToString()
                && tempData[UpdateTempDataKeys.FocusItemId.ToString()]?.ToString() == itemId.ToString();
            }

            if (buttonDirection == OrderDirection.Down && index != 0)
            {
                focus = tempData[UpdateTempDataKeys.Directon.ToString()]?.ToString() == OrderDirection.Down.ToString()
                && tempData[UpdateTempDataKeys.FocusItemId.ToString()]?.ToString() == itemId.ToString();
            }

            if (index == 0 || index == totalItems - 1)
            {
                focus = tempData[UpdateTempDataKeys.FocusItemId.ToString()]?.ToString() == itemId.ToString();
            }

            return focus;
        }
    }
}
