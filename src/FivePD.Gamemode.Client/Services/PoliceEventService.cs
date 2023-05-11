using CitizenFX.Core;
using FivePD.Common.Models;
using FivePD.Gamemode.Client.Interfaces;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class PoliceEventService : IPoliceEventService
    {
        private BasePoliceEvent _currentlyAttachedEvent;

        /// <inheritdoc />
        public bool IsAttachedToEvent()
        {
            return this._currentlyAttachedEvent is not null;
        }

        /// <inheritdoc />
        public bool IsEventMainPlayerEqualsToPlayer()
        {
            if (!this.IsAttachedToEvent())
            {
                return false;
            }

            return this._currentlyAttachedEvent.MainPlayer == Game.PlayerPed.NetworkId;
        }

        /// <inheritdoc />
        public BasePoliceEvent GetCurrentEvent()
        {
            return this._currentlyAttachedEvent;
        }

        /// <inheritdoc />
        public void SetCurrentEvent(BasePoliceEvent policeEvent)
        {
            this._currentlyAttachedEvent = policeEvent;
        }
    }
}