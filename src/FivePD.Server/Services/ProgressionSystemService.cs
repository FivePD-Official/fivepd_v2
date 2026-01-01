using FivePD.Server.Attributes;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Repositories;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class ProgressionSystemService : IProgressionSystemService
{
    private readonly UserRepository _userRepository;
    private readonly IPlayerDataService _playerDataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressionSystemService"/> class.
    /// </summary>
    /// <param name="userRepository">The <see cref="UserRepository"/> to use.</param>
    /// <param name="playerDataService">The <see cref="IPlayerDataService"/> to use.</param>
    public ProgressionSystemService(UserRepository userRepository, IPlayerDataService playerDataService)
    {
        this._userRepository = userRepository;
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
        this._userRepository.Update(user, x => { x.Xp = previousXp + experiencePoints; });

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