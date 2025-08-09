namespace FivePD.Server.Models;

/// <summary>
/// Represents the status of a ped's license.
/// </summary>
public enum PedLicenseStatus
{
    /// <summary>
    /// The license is valid.
    /// </summary>
    Valid,

    /// <summary>
    /// The license is expired.
    /// </summary>
    Expired,

    /// <summary>
    /// The license is suspended.
    /// </summary>
    Suspended,

    /// <summary>
    /// The license is revoked.
    /// </summary>
    Revoked,
}