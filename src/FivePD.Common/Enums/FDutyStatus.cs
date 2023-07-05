namespace FivePD.Common.Enums;

/// <summary>
/// An enum representing a player's duty status.
/// </summary>
public enum FDutyStatus
{
    /// <summary>
    /// The player is not on duty and should not be interacted with.
    /// </summary>
    OffDuty,

    /// <summary>
    /// The player is on duty and is receiving interactions.
    /// </summary>
    OnDuty,
}