using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SFA.DAS.AODP.Common.Extensions;

public static class EnumExtensions
{
    public static SelectList ToSelectList<TEnum>(this TEnum enumObj) where TEnum : Enum
    {
        var values = Enum.GetValues(typeof(TEnum))
                         .Cast<TEnum>()
                         .Select(e => new
                         {
                             Value = Convert.ToInt32(e),
                             Text = e.GetDescription() ?? e.ToString()
                         });
        return new SelectList(values, "Value", "Text");
    }


    // Helper method to get Description attribute if available
    public static string? GetDescription(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : null;
    }
}
