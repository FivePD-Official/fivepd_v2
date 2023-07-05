namespace FivePD.Common.Enums;

/// <summary>
/// An enum representing whether a player
/// is receiving callouts at the moment or not.
/// </summary>
public enum FPlayerAvailability
{
    /// <summary>
    /// The player is not on duty and should not be interacted with.
    /// </summary>
    Receiving,

    /// <summary>
    /// The player is on duty and is receiving interactions.
    /// </summary>
    NotReceiving,
}