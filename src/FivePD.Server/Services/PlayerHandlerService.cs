using System.Linq;
using CitizenFX.Core;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.Server.Services;

/// <inheritdoc />
[Obfuscate]
public class PlayerHandlerService : IPlayerHandlerService
{
    private readonly PlayerList _playerList;
    private readonly IDatabaseContextFactory _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerHandlerService"/> class.
    /// </summary>
    /// <param name="playerList">The player list.</param>
    /// <param name="dbContextFactory">The database context factory.</param>
    public PlayerHandlerService(PlayerList playerList, IDatabaseContextFactory dbContextFactory)
    {
        this._playerList = playerList;
        this._dbContextFactory = dbContextFactory;
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
        using var databaseContext = this._dbContextFactory.CreateScope().Context;
        return databaseContext.Users
            .WhereAnyIdMatches(player)
            .FirstOrDefault();
    }
}