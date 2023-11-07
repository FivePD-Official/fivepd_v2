using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class ProgressionSystemService : IProgressionSystemService
{
    private readonly DatabaseContext _databaseContext;
    private readonly IPlayerDataService _playerDataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressionSystemService"/> class.
    /// </summary>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    /// <param name="playerDataService">The <see cref="IPlayerDataService"/> to use.</param>
    public ProgressionSystemService(DatabaseContext databaseContext, IPlayerDataService playerDataService)
    {
        this._databaseContext = databaseContext;
        this._playerDataService = playerDataService;
    }

    /// <inheritdoc />
    public void AddExperiencePoints(Unit unit, int experiencePoints)
    {
        foreach (var player in unit.Players)
        {
            this.AddExperiencePoints(player, experiencePoints);
        }
    }

    /// <inheritdoc />
    public void AddExperiencePoints(FPlayer player, int experiencePoints)
    {
        var user = player.User;
        var previousXp = user.Xp;
        user.Xp += experiencePoints;
        this._databaseContext.SaveChanges();

        if (user.Xp >= previousXp)
        {
            this._playerDataService.IncreaseRank(player);
        }
        else
        {
            this._playerDataService.DecreaseRank(player);
        }
    }
}