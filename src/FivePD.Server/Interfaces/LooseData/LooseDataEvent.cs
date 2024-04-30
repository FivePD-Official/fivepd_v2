using FivePD.Server.EntityWrappers;
using FivePD.Server.Models.LooseData;

namespace FivePD.Server.Interfaces.LooseData;

/// <summary>
/// The event arguments for when an action has been made with a loose data entity.
/// </summary>
public sealed record LooseDataEvent
{
    /// <summary>
    /// Gets or sets the entity.
    /// </summary>
    public LooseDataEntity Entity { get; set; }

    /// <summary>
    /// Gets or sets the player who initiated the action.
    /// If the action is invoked using the API it will be null.
    /// </summary>
    public FPlayer Player { get; set; }
}