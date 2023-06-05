using FivePD.Gamemode.Common.Attributes;
using FivePD.Gamemode.Server.EntityWrappers;

namespace FivePD.Gamemode.Server.UIModels.Interfaces;

/// <summary>
/// Defines the events of <see cref="MenuButtonItem"/>.
/// </summary>
[NoUsage]
internal interface IMenuButtonItemWithEvents
{
    /// <summary>
    /// Raises the OnSelect event.
    /// </summary>
    /// <param name="player">The player who invoked this event.</param>
    void RaiseOnSelect(FPlayer player);
}