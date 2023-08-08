using FivePD.Server.EntityWrappers;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for managing the experience points of users.
/// </summary>
public interface IProgressionSystemService
{
    /// <summary>
    /// Adds the given XP to each player of the unit.
    /// Internally calls <see cref="IProgressionSystemService.AddExperiencePoints(FPlayer, int)"/>.
    /// </summary>
    /// <param name="unit">The unit.</param>
    /// <param name="experiencePoints">The XP to add.</param>
    public void AddExperiencePoints(Unit unit, int experiencePoints);

    /// <summary>
    /// Adds the given XP to the player and increases or decreases their rank.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="experiencePoints">The XP to add.</param>
    public void AddExperiencePoints(FPlayer player, int experiencePoints);
}