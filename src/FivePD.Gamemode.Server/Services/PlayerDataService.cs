using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.Attributes;
using FivePD.Gamemode.Server.Database;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models;
using FivePD.Gamemode.Server.Models.Requests;
using FivePD.Gamemode.Server.Models.Responses;
using FivePD.Gamemode.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FivePD.Gamemode.Server.Services;

/// <inheritdoc cref="FivePD.Gamemode.Server.Interfaces.IPlayerDataService" />
public class PlayerDataService : IPlayerDataService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly DatabaseContext _databaseContext;
    private readonly PlayerList _playerList;
    private readonly INotificationService _notificationService;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerDataService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    /// <param name="playerList">The <see cref="PlayerList"/> to use.</param>
    /// <param name="notificationService">The <see cref="INotificationService"/> to use.</param>
    /// <param name="featureFlagService">The <see cref="IFeatureFlagService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public PlayerDataService(
        EventHandlerDictionary eventHandlerDictionary,
        DatabaseContext databaseContext,
        PlayerList playerList,
        INotificationService notificationService,
        IFeatureFlagService featureFlagService,
        ILogger logger)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._databaseContext = databaseContext;
        this._playerList = playerList;
        this._notificationService = notificationService;
        this._featureFlagService = featureFlagService;
        this._logger = logger;
    }

    /// <inheritdoc />
    public event EventHandler<Rank> PlayerRankChanged;

    /// <inheritdoc />
    public event EventHandler<Division> PlayerDivisionChanged;

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this._eventHandlerDictionary["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(this.OnPlayerConnecting);

        foreach (var player in this._playerList)
        {
            this.CreatePlayerData(player);
            this.SubscribeToOnDutyStatusChange(player);
        }
    }

    /// <inheritdoc/>
    public User FindUserByLicense(Player player)
    {
        var license = player.Identifiers["license"];
        if (string.IsNullOrEmpty(license))
        {
            return null;
        }

        try
        {
            return this._databaseContext.Users
                .Include(user => user.Department)
                .WhereAnyIdMatches(player)
                .FirstOrDefault();
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    /// <inheritdoc />
    public bool AnyPlayerIdMatchesWithUserId(Player player, User user)
    {
        var identifierLicense = player.Identifiers["license"];
        var identifierDiscord = player.Identifiers["discord"];
        var identifierSteam = player.Identifiers["steam"];
        var identifierXbl = player.Identifiers["xbl"];
        var identifierLive = player.Identifiers["live"];

        return user.IdentifierLicense == identifierLicense ||
               user.IdentifierDiscord == identifierDiscord ||
               user.IdentifierSteam == identifierSteam ||
               user.IdentifierXbl == identifierXbl ||
               user.IdentifierLive == identifierLive;
    }

    private void OnPlayerConnecting([FromSource] Player player, string playerName, dynamic setKickReason, dynamic deferrals)
    {
        this.CreatePlayerData(player);
        this.SubscribeToOnDutyStatusChange(player);
    }

    private void SubscribeToOnDutyStatusChange(Player player)
    {
        if (!this._featureFlagService.IsFeatureEnabled(x => x.DutyNotification))
        {
            return;
        }

        var fPlayer = FPlayer.FromNetId(player.Handle);
        fPlayer.DutyStatus.Subscribe(status =>
        {
            // TODO: add correct notification with message and texture
            this._notificationService.ShowNotification(this._playerList, $"{fPlayer.GetUnderlyingPlayer().Name} is now {status}");
        });
    }

    private void CreatePlayerData(Player player)
    {
        var identifierLicense = player.Identifiers["license"] ?? string.Empty;
        var identifierDiscord = player.Identifiers["discord"] ?? string.Empty;
        var identifierSteam = player.Identifiers["steam"] ?? string.Empty;
        var identifierXbl = player.Identifiers["xbl"] ?? string.Empty;
        var identifierLive = player.Identifiers["live"] ?? string.Empty;

        if (string.IsNullOrEmpty(identifierLicense) &&
            string.IsNullOrEmpty(identifierDiscord) &&
            string.IsNullOrEmpty(identifierSteam) &&
            string.IsNullOrEmpty(identifierXbl) &&
            string.IsNullOrEmpty(identifierLive))
        {
            this._logger.Warning("Player \"{PlayerName}\" has no identifiers, they will not have any associated data stored.", player.Name);
        }

        try
        {
            var user = this._databaseContext.Users.WhereAnyIdMatches(player).FirstOrDefault();

            if (user is null)
            {
                this._databaseContext.Users.Add(new User
                {
                    Name = player.Name,
                    IdentifierLicense = player.Identifiers["license"] ?? string.Empty,
                    IdentifierDiscord = player.Identifiers["discord"] ?? string.Empty,
                    IdentifierSteam = player.Identifiers["steam"] ?? string.Empty,
                    IdentifierXbl = player.Identifiers["xbl"] ?? string.Empty,
                    IdentifierLive = player.Identifiers["live"] ?? string.Empty,
                });
                this._databaseContext.SaveChanges();
            }

            this._databaseContext.SaveChanges();
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    [UIEvent(Events.UI.Player.GetDetails, true)]
    private void GetPlayerDetails([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var license = player.Identifiers["license"];
        if (string.IsNullOrEmpty(license))
        {
            networkCallbackDelegate.Invoke(null);
            return;
        }

        try
        {
            var user = this._databaseContext.Users
                .AsNoTracking()
                .Include(user => user.Department)
                .Include(user => user.Division)
                .Include(user => user.Rank)
                .WhereAnyIdMatches(player)
                .FirstOrDefault();
            networkCallbackDelegate.Invoke(user is null ? null : Converter.SerializeObjectWithoutReferenceLoop(user));
        }
        catch (Exception e)
        {
            networkCallbackDelegate.Invoke(null);
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    [UIEvent(Events.UI.Player.ChangeDivision)]
    private void ChangePlayerDivisionFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var playerChangeRequest = expandoObject.ConvertTo<PlayerChangeRequest>();
            var user = this._databaseContext.Users.FirstOrDefault(user => user.Id == playerChangeRequest.UserId);
            if (user is null)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "UI.ERROR.UNEXPECTED",
                    Description = "UI.ERROR.DATABASE_ACCESS.TITLE",
                }.ToString());
                return;
            }

            Division division;
            if (playerChangeRequest.IdToChange == -1)
            {
                user.DivisionId = null;
                division = null;
            }
            else
            {
                division = this._databaseContext.Divisions.FirstOrDefault(division => division.Id == playerChangeRequest.IdToChange);
                if (division is null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "UI.ERROR.UNEXPECTED",
                        Description = "UI.ERROR.DATABASE_ACCESS.TITLE",
                    }.ToString());
                    return;
                }

                user.DivisionId = playerChangeRequest.IdToChange;
            }

            this._databaseContext.SaveChanges();
            this.PlayerDivisionChanged?.Invoke(this, division);
            networkCallbackDelegate.Invoke(new ActionResponse
            {
                Status = true,
                Title = "UI.LABEL.SUCCESS.CHANGE",
                Description = "UI.LABEL.DIVISION.SUCCESS.CHANGE",
                Params = new Dictionary<string, string>
                {
                    { "player", user.Name },
                },
            }.ToString());
        }
        catch (Exception e)
        {
            networkCallbackDelegate.Invoke(new ActionResponse
            {
                Status = false,
                Title = "UI.ERROR.UNEXPECTED",
                Description = "UI.ERROR.DATABASE_ACCESS.TITLE",
            }.ToString());
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    [UIEvent(Events.UI.Player.ChangeRank)]
    private void ChangePlayerRankFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var playerChangeRequest = expandoObject.ConvertTo<PlayerChangeRequest>();
            var user = this._databaseContext.Users.FirstOrDefault(user => user.Id == playerChangeRequest.UserId);
            if (user is null)
            {
                networkCallbackDelegate.Invoke(new ActionResponse
                {
                    Status = false,
                    Title = "UI.ERROR.UNEXPECTED",
                    Description = "UI.ERROR.DATABASE_ACCESS.TITLE",
                }.ToString());
                return;
            }

            Rank rank;
            if (playerChangeRequest.IdToChange == -1)
            {
                user.RankId = null;
                rank = null;
            }
            else
            {
                rank = this._databaseContext.Ranks.FirstOrDefault(division => division.Id == playerChangeRequest.IdToChange);
                if (rank is null)
                {
                    networkCallbackDelegate.Invoke(new ActionResponse
                    {
                        Status = false,
                        Title = "UI.ERROR.UNEXPECTED",
                        Description = "UI.ERROR.DATABASE_ACCESS.TITLE",
                    }.ToString());
                    return;
                }

                user.RankId = playerChangeRequest.IdToChange;
            }

            this._databaseContext.SaveChanges();
            this.PlayerRankChanged?.Invoke(this, rank);
            networkCallbackDelegate.Invoke(new ActionResponse
            {
                Status = true,
                Title = "UI.LABEL.SUCCESS.CHANGE",
                Description = "UI.LABEL.RANK.SUCCESS.CHANGE",
                Params = new Dictionary<string, string>
                {
                    { "player", user.Name },
                },
            }.ToString());
        }
        catch (Exception e)
        {
            networkCallbackDelegate.Invoke(new ActionResponse
            {
                Status = false,
                Title = "UI.ERROR.UNEXPECTED",
                Description = "UI.ERROR.DATABASE_ACCESS.TITLE",
            }.ToString());
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    [UIEvent(Events.UI.Player.GetAllPageable)]
    private void GetAllPageable(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<GetSetRequest>();
            var page = this._databaseContext.Users
                .AsNoTracking()
                .Paginate(request.Page, new Filters<User>());

            networkCallbackDelegate.Invoke(Converter.SerializeObjectWithoutReferenceLoop(page));
        }
        catch (Exception)
        {
            networkCallbackDelegate.Invoke(null);
        }
    }

    [UIEvent(Events.UI.Player.ChangeRole)]
    private void ChangeRole(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var request = expandoObject.ConvertTo<ChangePlayerRoleRequest>();
            var user = this._databaseContext.Users.FirstOrDefault(x => x.Id == request.UserId);
            if (user is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            user.Role = request.Roles.Sum();
            this._databaseContext.SaveChanges();
            networkCallbackDelegate.InvokeWithResponse(
                true,
                "UI.LABEL.SUCCESS.CHANGE",
                "UI.LABEL.PLAYER.SUCCESS.CHANGE_ROLE",
                new Dictionary<string, string>
                {
                    { "name", user.Name },
                });
        }
        catch (Exception)
        {
            networkCallbackDelegate.Invoke(null);
        }
    }
}