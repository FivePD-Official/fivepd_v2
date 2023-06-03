using System.Linq;
using CitizenFX.Core;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.Extensions;

/// <summary>
/// Contains extension methods for <see cref="IQueryable{User}"/>.
/// </summary>
public static class IQueryableUserExtensions
{
    /// <summary>
    /// Filters out all users where any identifier matches with any identifier of the specified player.
    /// </summary>
    /// <param name="source">The source query.</param>
    /// <param name="player">The player.</param>
    /// <returns>The new query.</returns>
    public static IQueryable<User> WhereAnyIdMatches(this IQueryable<User> source, Player player)
    {
        var identifierLicense = player.Identifiers["license"] ?? string.Empty;
        var identifierDiscord = player.Identifiers["discord"] ?? string.Empty;
        var identifierSteam = player.Identifiers["steam"] ?? string.Empty;
        var identifierXbl = player.Identifiers["xbl"] ?? string.Empty;
        var identifierLive = player.Identifiers["live"] ?? string.Empty;

        source = source.Where(user => user.IdentifierLicense == identifierLicense ||
                                      user.IdentifierDiscord == identifierDiscord ||
                                      user.IdentifierSteam == identifierSteam ||
                                      user.IdentifierXbl == identifierXbl ||
                                      user.IdentifierLive == identifierLive);
        return source;
    }
}