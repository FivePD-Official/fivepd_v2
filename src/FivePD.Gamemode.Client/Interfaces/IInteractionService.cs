using CitizenFX.Core;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Provides access to functionality concerning
    /// interacting with entities in the game world.
    /// </summary>
    public interface IInteractionService
    {
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