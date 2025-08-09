namespace FivePD.Server.Models;

/// <summary>
/// Represents the status of a vehicle document.
/// </summary>
public enum VehicleDocumentStatus
{
    /// <summary>
    /// The document is valid.
    /// </summary>
    Valid,

    /// <summary>
    /// The document is expired.
    /// </summary>
    Expired,

    /// <summary>
    /// The document is suspended.
    /// </summary>
    Suspended,

    /// <summary>
    /// The document is revoked.
    /// </summary>
    Revoked,
}