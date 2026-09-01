using SFA.DAS.AODP.Models.Qualifications;

namespace SFA.DAS.AODP.Web.Models.Qualifications;

/// <summary>
/// Extension methods for KeyFieldChange to add key field-specific metadata.
/// </summary>
public static class KeyFieldChangeExtensions
{
    /// <summary>
    /// Create a KeyFieldChange from a KeyField definition with old and new values.
    /// </summary>
    public static KeyFieldChange CreateFromKeyField(
        this KeyField keyField,
        string? oldValue,
        string? newValue)
    {
        return new KeyFieldChange(keyField, oldValue, newValue);
    }

    /// <summary>
    /// Get the key field that corresponds to this change by name (for display purposes).
    /// Returns null if this change doesn't match any key field definition.
    /// </summary>
    public static KeyField? GetKeyField(this KeyFieldChange change)
    {
        return KeyField.All.FirstOrDefault(kf => kf.DisplayName == change.KeyField.DisplayName);
    }

    /// <summary>
    /// Get the priority of this change if it's a key field change, otherwise Green.
    /// </summary>
    public static KeyFieldPriority GetPriority(this KeyFieldChange change)
    {
        var keyField = change.GetKeyField();
        return keyField?.Priority ?? KeyFieldPriority.Green;
    }
}
