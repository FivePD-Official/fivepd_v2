using FivePD.Server.EntityWrappers;
using FivePD.Server.UIModels.MenuModels;

namespace FivePD.Server.UIModels.Interfaces;

/// <summary>
/// Defines the events of <see cref="MenuListItem"/>.
/// </summary>
internal interface IMenuListItemWithEvents
{
    /// <summary>
    /// Raises the OnIndexChange event.
    /// </summary>
    /// <param name="player">The player who invoked this event.</param>
    /// <param name="index">The currently selected index.</param>
    public void RaiseOnIndexChange(FPlayer player, int index);

    /// <summary>
    /// Raises the OnSelect event.
    /// </summary>
    /// <param name="player">The player who invoked this event.</param>
    /// <param name="index">The currently selected index.</param>
    void RaiseOnSelect(FPlayer player, int index);
}