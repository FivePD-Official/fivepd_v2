// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using CitizenFX.Core;
using FivePD.Common.Models;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Provides access to functionality concerning
    /// interacting with entities in the game world.
    /// </summary>
    public interface IInteractionService
    {
        /// <summary>
        /// Registers related keybinds.
        /// </summary>
        public void RegisterKeybinds();

        /// <summary>
        /// Trys to locate a target entity
        /// and alert them to stop.
        /// </summary>
        public void TryAlertEntity();

        /// <summary>
        /// Attempts to locate an entity based
        /// on factors such as the player's forward
        /// position, camera position, and target entity.
        /// </summary>
        /// <returns>The closest entity.</returns>
        public Entity GetLikelyTargetEntity();

        /// <summary>
        /// Attempts to locate an entity of the given type
        /// based on factors such as the player's forward
        /// position, camera position, and target entity.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Entity"/> to
        /// search for.</typeparam>
        /// <returns>The closest entity of the type.</returns>
        public T GetLikelyTargetEntity<T>()
            where T : Entity;
    }
}