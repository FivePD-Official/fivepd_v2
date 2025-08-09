namespace FivePD.Server.Models;

/// <summary>
/// Enumeration representing the item types used for callsign formats.
/// </summary>
public enum CallsignFormatItemType
{
    /// <summary>
    /// A dash.
    /// </summary>
    Dash,

    /// <summary>
    /// A fixed character.
    /// </summary>
    FixedCharacter,

    /// <summary>
    /// A random letter.
    /// </summary>
    RandomLetter,

    /// <summary>
    /// A random number.
    /// </summary>
    RandomNumber,

    /// <summary>
    /// A word.
    /// </summary>
    Word,
}