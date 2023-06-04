using FivePD.Gamemode.Common.Attributes;
using FivePD.Gamemode.Common.Models;

namespace FivePD.Gamemode.Common.UIModels.Interfaces;

/// <summary>
/// Defines the events of <see cref="MenuCheckboxItem"/>.
/// </summary>
[NoUsage]
internal interface IMenuCheckboxItemWithEvents
{
    /// <summary>
    /// Raises the OnSelect event.
    /// </summary>
    /// <param name="player">The player who invoked this event.</param>
    /// <param name="checked">The value of the checkbox.</param>
    void RaiseOnSelect(FPlayer player, bool @checked);
}