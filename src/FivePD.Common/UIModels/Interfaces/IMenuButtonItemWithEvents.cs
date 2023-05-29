using FivePD.Common.Attributes;
using FivePD.Common.Models;

namespace FivePD.Common.UIModels.Interfaces
{
    /// <summary>
    /// Defines the events of <see cref="MenuButtonItem"/>.
    /// </summary>
    [NoUsage]
    public interface IMenuButtonItemWithEvents
    {
        /// <summary>
        /// Raises the OnSelect event.
        /// </summary>
        /// <param name="player">The player who invoked this event.</param>
        void RaiseOnSelect(FPlayer player);
    }
}