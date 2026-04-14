namespace SFA.DAS.AODP.Web.TagHelpers;

/// <summary>
/// Defines a custom exception type for errors that occur during the rendering of TagHelpers. This exception can be used to provide more specific error messages related to TagHelper processing,
/// making it easier to identify and troubleshoot issues in the Razor views where TagHelpers are used.
/// </summary>
/// <param name="tagHelperName">The name of the TagHelper where the error occurred.</param>
/// <param name="message">The error message describing the issue.</param>
[ExcludeFromCodeCoverage]
public class TagHelperRenderingException(string tagHelperName, string message)
    : Exception($"An error occurred in the '{tagHelperName}' tag helper: {message}")
{
}