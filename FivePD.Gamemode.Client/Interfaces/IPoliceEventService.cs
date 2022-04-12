// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using FivePD.Common.Models;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Responsible for handling the player's currently attached <see cref="BasePoliceEvent"/>.
    /// </summary>
    public interface IPoliceEventService
    {
        /// <summary>
        /// Stores the event on the server.
        /// </summary>
        /// <param name="policeEvent">The event to store.</param>
        public void Initiate(BasePoliceEvent policeEvent);

        /// <summary>
        /// Cancels the currently attached event, it'll also be deleted on the server,
        /// and canceled for all attached players as well.
        /// </summary>
        public void Cancel();

        /// <summary>
        /// Gets if the player is attached to an event.
        /// </summary>
        /// <returns>A boolean whether the player is attached to an event.</returns>
        public bool IsAttachedToEvent();

        /// <summary>
        /// Gets if the player's attached event's main player is the player.
        /// </summary>
        /// <returns>A boolean.</returns>
        public bool IsEventMainPlayerEqualsToPlayer();

        /// <summary>
        /// Gets the currently attached event.
        /// </summary>
        /// <returns>The current event.</returns>
        public BasePoliceEvent GetCurrentEvent();

        /// <summary>
        /// Sets the currently attached event.
        /// </summary>
        /// <param name="policeEvent">The new event.</param>
        public void SetCurrentEvent(BasePoliceEvent policeEvent);
    }
}