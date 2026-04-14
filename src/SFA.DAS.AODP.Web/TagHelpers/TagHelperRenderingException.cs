namespace SFA.DAS.AODP.Web.TagHelpers;

public class TagHelperRenderingException(string tagHelperName, string message)
    : Exception($"An error occurred in the '{tagHelperName}' tag helper: {message}")
{
}