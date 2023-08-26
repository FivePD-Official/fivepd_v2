namespace FivePD.Server.Models.LooseData;

/// <summary>
/// Defines the type of the attribute.
/// Mainly used for displaying the correct input fields in the NUI.
/// </summary>
public enum LooseDataType
{
    /// <summary>
    /// This type will display a text input field in forms.
    /// </summary>
    ShortString,

    /// <summary>
    /// This type will display a textarea in forms.
    /// </summary>
    LongString,

    /// <summary>
    /// This type will display a number input field in forms.
    /// </summary>
    Integer,

    /// <summary>
    /// This type will display a checkbox in forms.
    /// </summary>
    Bool,

    /// <summary>
    /// This type will display a date input field in forms.
    /// </summary>
    Date,
}