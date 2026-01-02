using System.Linq;
using CitizenFX.Core;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Repositories;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class PlayerHandlerService : IPlayerHandlerService
{
    private readonly PlayerList _playerList;
    private readonly UserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerHandlerService"/> class.
    /// </summary>
    /// <param name="playerList">The player list.</param>
    /// <param name="userRepository">The userrepository.</param>
    public PlayerHandlerService(PlayerList playerList, UserRepository userRepository)
    {
        this._playerList = playerList;
        this._userRepository = userRepository;
    }

    /// <inheritdoc />
    public Player FindPlayerOfUser(User user)
    {
        return this._playerList.FirstOrDefault(player =>
        {
            var identifierLicense = player.Identifiers["license"] ?? string.Empty;
            var identifierDiscord = player.Identifiers["discord"] ?? string.Empty;
            var identifierSteam = player.Identifiers["steam"] ?? string.Empty;
            var identifierXbl = player.Identifiers["xbl"] ?? string.Empty;
            var identifierLive = player.Identifiers["live"] ?? string.Empty;

            return user.IdentifierLicense == identifierLicense ||
                   user.IdentifierDiscord == identifierDiscord ||
                   user.IdentifierSteam == identifierSteam ||
                   user.IdentifierXbl == identifierXbl ||
                   user.IdentifierLive == identifierLive;
        });
    }

    /// <inheritdoc />
    public User FindUserOfPlayer(Player player)
    {
        return this._userRepository.FindByLicense(player);
    }
}