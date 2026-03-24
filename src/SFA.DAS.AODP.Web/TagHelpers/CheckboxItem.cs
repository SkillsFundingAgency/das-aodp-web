namespace SFA.DAS.AODP.Web.TagHelpers;

/// <summary>
/// Defines the model for a checkbox item to be sued with the <see cref="CheckboxesTagHelper"/>.
/// </summary>
public class CheckboxItem
{
    /// <summary>
    /// The text to set for the associated label.
    /// </summary>
    public string LabelText { get; set; } = null!;

    /// <summary>
    /// The value attribute to set.
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// Whether the checkbox is checked or not.
    /// </summary>
    public bool IsChecked { get; set; }

    /// <summary>
    /// Sets the checkbox as being checked.
    /// </summary>
    public void SetChecked()
    {
        IsChecked = true;
    }
}