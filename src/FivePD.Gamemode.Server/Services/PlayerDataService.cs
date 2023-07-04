using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Server.Attributes;
using FivePD.Gamemode.Server.Database;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models;
using FivePD.Gamemode.Server.Models.DTO;
using FivePD.Gamemode.Server.Models.Requests;
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
    private readonly IConfigService _configService;
    private readonly ILogger _logger;
    private readonly IUnitService _unitService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerDataService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    /// <param name="playerList">The <see cref="PlayerList"/> to use.</param>
    /// <param name="notificationService">The <see cref="INotificationService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="unitService">The <see cref="IUnitService"/> to use.</param>
    public PlayerDataService(
        EventHandlerDictionary eventHandlerDictionary,
        DatabaseContext databaseContext,
        PlayerList playerList,
        INotificationService notificationService,
        IConfigService configService,
        ILogger logger,
        IUnitService unitService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._databaseContext = databaseContext;
        this._playerList = playerList;
        this._notificationService = notificationService;
        this._configService = configService;
        this._logger = logger;
        this._unitService = unitService;
    }

    /// <inheritdoc />
    public event EventHandler<Rank> PlayerRankChanged;

    /// <inheritdoc />
    public event EventHandler<Division> PlayerDivisionChanged;

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Player.OnConnected] += new Action<Player>(this.OnPlayerConnected);

        foreach (var player in this._playerList)
        {
            this.CreatePlayerData(player);
            this.SubscribeToOnDutyStatusChange(player);
        }
    }

    /// <inheritdoc/>
    public User FindUserByLicense(Player player, bool withChangeTracking = true)
    {
        try
        {
            var query = this._databaseContext.Users.AsQueryable();
            if (!withChangeTracking)
            {
                query = query.AsNoTracking();
            }

            return query
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

    private void OnPlayerConnected([FromSource] Player player)
    {
        this.CreatePlayerData(player);
        this.SubscribeToOnDutyStatusChange(player);
    }

    private void SubscribeToOnDutyStatusChange(Player player)
    {
        if (!this._configService.GeneralConfig.FeatureFlags.DutyNotification)
        {
            return;
        }

        var fPlayer = FPlayer.FromNetId(player.Handle);
        fPlayer.DutyStatus.Value = FDutyStatus.OffDuty;
        fPlayer.DutyStatus.Subscribe(status => this.OnPlayerDutyStatusChanged(player, status), false);
    }

    private void OnPlayerDutyStatusChanged(Player player, FDutyStatus status)
    {
        var fPlayer = FPlayer.FromNetId(player.Handle);

        this._notificationService.ShowNotificationWithTexture(
            this._playerList.Where(p => p.Handle != player.Handle),
            "CHAR_CALL911",
            "LABEL.DISPATCH",
            "LABEL.STATUS",
            "PLAYER_STATUS.CHANGE",
            new Dictionary<string, string>
            {
                ["name"] = player.Name,
                ["status"] = status switch
                {
                    FDutyStatus.OffDuty => "PLAYER_STATUS.OFF_DUTY.GENERIC",
                    FDutyStatus.OnDuty => "PLAYER_STATUS.ON_DUTY.GENERIC",
                    _ => "Unknown",
                },
            });

        var playerDetails = this.GetPlayerDetails(player);
        if (status == FDutyStatus.OffDuty)
        {
            this._unitService.RemovePlayerFromUnit(fPlayer);
        }
        else
        {
            this._unitService.CreateUnit(playerDetails.Callsign, fPlayer);
        }
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
            return;
        }

        try
        {
            var userCount = this._databaseContext.Users.Count();
            var existingUser = this._databaseContext.Users.WhereAnyIdMatches(player).FirstOrDefault();
            if (existingUser is null)
            {
                var user = new User
                {
                    Name = player.Name,
                    IdentifierLicense = player.Identifiers["license"] ?? string.Empty,
                    IdentifierDiscord = player.Identifiers["discord"] ?? string.Empty,
                    IdentifierSteam = player.Identifiers["steam"] ?? string.Empty,
                    IdentifierXbl = player.Identifiers["xbl"] ?? string.Empty,
                    IdentifierLive = player.Identifiers["live"] ?? string.Empty,
                };
                if (userCount == 0)
                {
                    user.Role = (int)UserRole.Admin;
                }

                this._databaseContext.Users.Add(user);
            }

            this._databaseContext.SaveChanges();
        }
        catch (Exception e)
        {
            throw new DatabaseUsageException("Failed to access database", e);
        }
    }

    private User GetPlayerDetails(Player player)
    {
        try
        {
            return this._databaseContext.Users
                .AsNoTracking()
                .WhereAnyIdMatches(player)
                .FirstOrDefault();
        }
        catch (Exception)
        {
            return null;
        }
    }

    [UIEvent(Events.UI.Player.GetDetails, true)]
    private void GetPlayerDetailsUI([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
        {
            var user = this._databaseContext.Users
                .AsNoTracking()
                .Include(user => user.Department)
                .Include(user => user.Division)
                .Include(user => user.Rank)
                .WhereAnyIdMatches(player)
                .FirstOrDefault();
            networkCallbackDelegate.InvokeWithJson(user is null
                ? null
                : new PlayerDetailsDTO(player.Handle, user, FPlayer.FromNetId(player.Handle).DutyStatus.Value));
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