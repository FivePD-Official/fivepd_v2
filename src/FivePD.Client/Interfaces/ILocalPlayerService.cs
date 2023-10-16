using FivePD.Client.DTO;
using FivePD.Client.Models;
using FivePD.Common.Enums;
using FivePD.Common.Models;

namespace FivePD.Client.Interfaces;

/// <summary>
/// Provides access to the client's player.
/// </summary>
internal interface ILocalPlayerService
{
    /// <summary>
    /// Gets the local <see cref="FPlayer"/>.
    /// </summary>
    public FPlayer Player { get; }

    /// <summary>
    /// Gets the player's unit's availability.
    /// </summary>
    public FUnitAvailability Availability { get; }

    /// <summary>
    /// Gets the player's unit's status.
    /// </summary>
    public string Status { get; }

    /// <summary>
    /// Gets the player's current information.
    /// </summary>
    public InformationUpdateDTO Information { get; }

    /// <summary>
    /// Updated the information callout.
    /// </summary>
    /// <param name="clientIncidentWrapper">The new callout.</param>
    public void UpdateInformationCallout(ClientIncidentWrapper clientIncidentWrapper);

    /// <summary>
    /// Sends the information to the UI.
    /// </summary>
    public void UpdateInformationInUI();
}