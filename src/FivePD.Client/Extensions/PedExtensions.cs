using CitizenFX.Core;

namespace FivePD.Client.Extensions;

/// <summary>
/// Holds extension methods for CitizenFX.Core.Ped.
/// </summary>
internal static class PedExtensions
{
    /// <summary>
    /// Sets the required flags for the ped to ignore default GTA events.
    /// </summary>
    /// <param name="ped">The ped to set the flags for.</param>
    /// <param name="state">True if it should ignore GTA events, otherwise false.</param>
    public static void KeepTask(this Ped ped, bool state)
    {
        ped.AlwaysKeepTask = state;
        ped.BlockPermanentEvents = state;
    }

    /// <summary>
    /// Sets a ped's relationship to the players.
    /// </summary>
    /// <param name="ped">The ped to set the relationship for.</param>
    /// <param name="relationship">The relationship to set.</param>
    public static void SetRelationShip(this Ped ped, Relationship relationship)
    {
        ped.RelationshipGroup.SetRelationshipBetweenGroups(Game.PlayerPed.RelationshipGroup, relationship, true);
    }
}