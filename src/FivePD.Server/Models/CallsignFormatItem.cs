namespace FivePD.Server.Models;

/// <summary>
/// Represents a format item used in a callsign format.
/// </summary>
public class CallsignFormatItem
{
    /// <summary>
    /// Gets or sets the type of the item.
    /// </summary>
    public CallsignFormatItemType Type { get; set; }

    /// <summary>
    /// Gets or sets the value used for the "Fixed character" type.
    /// </summary>
    public string Value { get; set; }
}