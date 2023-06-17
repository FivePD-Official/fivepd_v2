using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.UIModels.MenuModels;

namespace FivePD.Gamemode.Server.UIModels.Interfaces;

/// <summary>
/// Defines the events of <see cref="MenuButtonItem"/>.
/// </summary>
internal interface IMenuButtonItemWithEvents
{
    /// <summary>
    /// Raises the OnSelect event.
    /// </summary>
    /// <param name="player">The player who invoked this event.</param>
    void RaiseOnSelect(FPlayer player);
}