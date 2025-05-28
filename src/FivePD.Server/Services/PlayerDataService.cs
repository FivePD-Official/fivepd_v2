using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Builders;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Events;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IPlayerDataService" />
[Obfuscate]
public class PlayerDataService : IPlayerDataService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly DatabaseContext _databaseContext;
    private readonly PlayerList _playerList;
    private readonly IGameNotificationService _notificationService;
    private readonly IConfigService _configService;
    private readonly ILogger _logger;
    private readonly ILocalizationService _localizationService;
    private readonly IPlayerHandlerService _playerHandlerService;
    private readonly IInternalPedStorageService _internalPedStorageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerDataService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    /// <param name="playerList">The <see cref="PlayerList"/> to use.</param>
    /// <param name="notificationService">The <see cref="IGameNotificationService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
    /// <param name="playerHandlerService">The <see cref="IPlayerHandlerService"/> to use.</param>
    /// <param name="pedStorageService">The <see cref="IPedStorageService"/> to use.</param>
    public PlayerDataService(
        EventHandlerDictionary eventHandlerDictionary,
        DatabaseContext databaseContext,
        PlayerList playerList,
        IGameNotificationService notificationService,
        IConfigService configService,
        ILogger logger,
        ILocalizationService localizationService,
        IPlayerHandlerService playerHandlerService,
        IPedStorageService pedStorageService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._databaseContext = databaseContext;
        this._playerList = playerList;
        this._notificationService = notificationService;
        this._configService = configService;
        this._logger = logger;
        this._localizationService = localizationService;
        this._playerHandlerService = playerHandlerService;
        this._internalPedStorageService = (IInternalPedStorageService)pedStorageService;
    }

    /// <inheritdoc/>
    public event EventHandler<PlayerDutyStatusChangeEventArgs> PlayerDutyStatusChanged;

    /// <inheritdoc />
    public event EventHandler<PlayerRankChangeEventArgs> PlayerRankChanged;

    /// <inheritdoc />
    public event EventHandler<PlayerDivisionChangeEventArgs> PlayerDivisionChanged;

    /// <inheritdoc />
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.Player.OnConnected] += new Action<Player>(this.OnPlayerConnected);
        this._eventHandlerDictionary[Events.Player.GetDismissedWelcomeModal] += new Action<Player, NetworkCallbackDelegate>(this.GetDismissedWelcomeModal);
        this._eventHandlerDictionary[Events.Player.DismissedWelcomeModal] += new Action<Player>(this.DismissedWelcomeModal);
        this._eventHandlerDictionary[Events.Player.GetRole] += new Action<Player, NetworkCallbackDelegate>(this.GetRole);
    }

    /// <inheritdoc/>
    public User FindUserByLicense(Player player, bool withChangeTracking = true)
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

    /// <inheritdoc />
    public void IncreaseRank(FPlayer player)
    {
        if (!this._configService.GeneralConfig.Features.ProgressionSystem)
        {
            return;
        }

        var user = player.User;
        if (user.FixedRank)
        {
            return;
        }

        if (user.Department is null)
        {
            return;
        }

        this._databaseContext
            .Entry(user)
            .Reference(x => x.Rank)
            .Load();

        List<Rank> ranksToUse;
        if (user.Department.UseGlobalRanks)
        {
            ranksToUse = this._databaseContext.Ranks.Where(x => x.Global).ToList();
        }
        else
        {
            this._databaseContext
                .Entry(user.Department)
                .Collection(x => x.DepartmentRanks)
                .Load();
            ranksToUse = user.Department.DepartmentRanks.Select(x => x.Rank).ToList();
        }

        var ranks = ranksToUse.OrderBy(x => x.RequiredXp).ToList();
        var currentRankRequiredXp = user.Rank?.RequiredXp ?? user.Xp;
        var nextRank = ranks.FirstOrDefault(x => x.RequiredXp > currentRankRequiredXp);
        if (nextRank is null)
        {
            return;
        }

        if (nextRank.RequiredXp > user.Xp)
        {
            return;
        }

        user.RankId = nextRank.Id;
        this._databaseContext.SaveChanges();
        this.PlayerRankChanged?.Invoke(this, new PlayerRankChangeEventArgs
        {
            Rank = nextRank,
            Player = player,
        });
    }

    /// <inheritdoc/>
    public void DecreaseRank(FPlayer player)
    {
        if (!this._configService.GeneralConfig.Features.ProgressionSystem)
        {
            return;
        }

        var user = player.User;
        if (user.FixedRank)
        {
            return;
        }

        if (user.Department is null)
        {
            return;
        }

        this._databaseContext
            .Entry(user)
            .Reference(x => x.Rank)
            .Load();

        List<Rank> ranksToUse;
        if (user.Department.UseGlobalRanks)
        {
            ranksToUse = this._databaseContext.Ranks.Where(x => x.Global).ToList();
        }
        else
        {
            this._databaseContext
                .Entry(user.Department)
                .Collection(x => x.DepartmentRanks)
                .Load();
            ranksToUse = user.Department.DepartmentRanks.Select(x => x.Rank).ToList();
        }

        var ranks = ranksToUse.OrderBy(x => x.RequiredXp).ToList();
        var currentRankRequiredXp = user.Rank?.RequiredXp ?? user.Xp;
        var previousRank = ranks.LastOrDefault(x => x.RequiredXp < currentRankRequiredXp);
        if (previousRank is null)
        {
            return;
        }

        if (previousRank.RequiredXp <= user.Xp)
        {
            return;
        }

        user.RankId = previousRank.Id;
        this._databaseContext.SaveChanges();
        this.PlayerRankChanged?.Invoke(this, new PlayerRankChangeEventArgs
        {
            Rank = previousRank,
            Player = player,
        });
    }

    private void OnPlayerConnected([FromSource] Player player)
    {
        this.CreatePlayerData(player);
        this.SubscribeToOnDutyStatusChange(player);
        this._internalPedStorageService.InternalPeds[player.Character.NetworkId] = new FPed(new FPedBuilder(new CommonFPedBuilder(player.Character.NetworkId)));
    }

    private void SubscribeToOnDutyStatusChange(Player player)
    {
        if (!this._configService.GeneralConfig.Features.DutyNotification)
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
        if (fPlayer is null)
        {
            return;
        }

        foreach (var p in this._playerList.Where(p => p.Handle != player.Handle))
        {
            // TODO: replace with gamenotificationservice
            this._notificationService.Notify(
                p,
                new GameNotificationOptions
                {
                    Title = "LABEL.DISPATCH",
                    Message = "PLAYER_STATUS.CHANGE",
                    MessageParams = new Dictionary<string, string>
                    {
                        ["name"] = player.Name,
                        ["status"] = status switch
                        {
                            FDutyStatus.OffDuty => "PLAYER_STATUS.OFF_DUTY.GENERIC",
                            FDutyStatus.OnDuty => "PLAYER_STATUS.ON_DUTY.GENERIC",
                            _ => "Unknown",
                        },
                    },
                });
        }

        this.PlayerDutyStatusChanged?.Invoke(
            this,
            new PlayerDutyStatusChangeEventArgs
            {
                Player = fPlayer,
                Status = status,
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
            return;
        }

        var userCount = this._databaseContext.Users.Count();
        var existingUser = this._databaseContext.Users.WhereAnyIdMatches(player).FirstOrDefault();
        if (existingUser is null)
        {
            var user = new User
            {
                FirstLogin = true,
                AccessFivePD = this._configService.GeneralConfig.Features.AutoEnableFivePDAccess,
                Name = player.Name,
                IdentifierLicense = player.Identifiers["license"] ?? string.Empty,
                IdentifierDiscord = player.Identifiers["discord"] ?? string.Empty,
                IdentifierSteam = player.Identifiers["steam"] ?? string.Empty,
                IdentifierXbl = player.Identifiers["xbl"] ?? string.Empty,
                IdentifierLive = player.Identifiers["live"] ?? string.Empty,
                AudioVolume = 0.1,
            };
            if (userCount == 0)
            {
                user.Role = (int)UserRole.Admin;
            }

            this._databaseContext.Users.Add(user);
            this._databaseContext.SaveChanges();
        }
        else
        {
            if (!existingUser.FirstLogin)
            {
                return;
            }

            existingUser.FirstLogin = false;
            this._databaseContext.SaveChanges();
        }
    }

    private void GetRole([FromSource] Player player, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var user = this._playerHandlerService.FindUserOfPlayer(player);
        networkCallbackDelegate.Invoke(user?.Role ?? 0);
    }

    [UIEvent(Events.UI.Player.GetDetails, true)]
    private void GetPlayerDetailsUI([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var user = this._databaseContext.Users
            .AsNoTracking()
            .Include(user => user.Department)
            .Include(user => user.Division)
            .Include(user => user.Rank)
            .Include(user => user.CallsignFormat)
            .WhereAnyIdMatches(player)
            .FirstOrDefault();

        if (user is null)
        {
            networkCallbackDelegate.InvokeWithJson(null);
            return;
        }

        var fPlayer = FPlayer.FromNetId(player.Handle);
        var unit = fPlayer.Unit;

        networkCallbackDelegate.InvokeWithJson(new PlayerDetailsDTO
        {
            NetworkId = player.Handle,
            User = user,
            DutyStatus = fPlayer.DutyStatus.Value,
            UnitCallsign = unit is null ? string.Empty : unit.Callsign,
        });
    }

    private void GetDismissedWelcomeModal([FromSource] Player player, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var user = this._databaseContext.Users
            .AsNoTracking()
            .WhereAnyIdMatches(player)
            .FirstOrDefault();
        if (user is null)
        {
            networkCallbackDelegate.Invoke(false);
            return;
        }

        networkCallbackDelegate.Invoke(user.DismissedWelcomeMessage);
    }

    private void DismissedWelcomeModal([FromSource] Player player)
    {
        var user = this._databaseContext.Users
            .WhereAnyIdMatches(player)
            .FirstOrDefault();
        if (user is null)
        {
            return;
        }

        user.DismissedWelcomeMessage = true;
        this._databaseContext.SaveChanges();
    }

    [UIEvent(Events.UI.Player.ChangeDivision, true)]
    private void ChangePlayerDivisionFromNui([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var playerChangeRequest = expandoObject.ConvertTo<PlayerChangeRequest>();
        var user = this._databaseContext.Users.FirstOrDefault(user => user.Id == playerChangeRequest.UserId);
        if (user is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
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
            division = this._databaseContext.Divisions.FirstOrDefault(x => x.Id == playerChangeRequest.IdToChange);
            if (division is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            user.DivisionId = playerChangeRequest.IdToChange;
        }

        this._databaseContext.SaveChanges();
        this.PlayerDivisionChanged?.Invoke(this, new PlayerDivisionChangeEventArgs
        {
            Division = division,
            Player = FPlayer.FromPlayer(player),
        });
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.CHANGE",
            "UI.LABEL.DIVISION.SUCCESS.CHANGE",
            new Dictionary<string, string>
            {
                { "player", user.Name },
            });
    }

    [UIEvent(Events.UI.Player.ChangeRank, true)]
    private void ChangePlayerRankFromNui([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var playerChangeRequest = expandoObject.ConvertTo<PlayerChangeRequest>();
        var user = this._databaseContext.Users.FirstOrDefault(user => user.Id == playerChangeRequest.UserId);
        if (user is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
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
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            user.RankId = playerChangeRequest.IdToChange;
        }

        this._databaseContext.SaveChanges();
        this.PlayerRankChanged?.Invoke(this, new PlayerRankChangeEventArgs
        {
            Rank = rank,
            Player = FPlayer.FromPlayer(player),
        });
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.CHANGE",
            "UI.LABEL.RANK.SUCCESS.CHANGE",
            new Dictionary<string, string>
            {
                { "player", user.Name },
            });
    }

    [UIEvent(Events.UI.Player.ChangeFixedRank)]
    private void ChangePlayerFixedRankFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<PlayerChangeFixedRankRequest>();
        var user = this._databaseContext.Users.FirstOrDefault(user => user.Id == request.UserId);
        if (user is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        user.FixedRank = request.FixedRank;
        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(
            true,
            "UI.LABEL.SUCCESS.CHANGE",
            request.FixedRank ? "UI.LABEL.PLAYER.SUCCESS.CHANGE_FIXED_RANK" : "UI.LABEL.PLAYER.SUCCESS.CHANGE_NOT_FIXED_RANK",
            new Dictionary<string, string>
            {
                { "name", user.Name },
            });
    }

    [UIEvent(Events.UI.Player.GetAllPageable)]
    private void GetAllPageable(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<GetSetRequest>();
        var page = this._databaseContext.Users
            .AsNoTracking()
            .Paginate(request.Page, new Filters<User>());

        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.Player.ChangeRole)]
    private void ChangeRole(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
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

        var playerOfUser = this._playerHandlerService.FindPlayerOfUser(user);
        playerOfUser?.TriggerEvent(Events.Player.OnRoleChange, user.Role);
    }

    [UIEvent(Events.User.GetAllSimple)]
    private void GetAllSimple(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        networkCallbackDelegate.InvokeWithJson(this._databaseContext.Users.Select(x => new SimpleUserDTO
        {
            Id = x.Id,
            Name = x.Name,
        }));
    }

    [UIEvent(Events.UI.Player.UpdateDetails, true)]
    private void UpdatePlayerDetailsUI([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<UpdatePlayerDetailsRequest>();
        var user = this._databaseContext.Users
            .Include(x => x.CallsignFormat)
            .Include(x => x.Department)
            .ThenInclude(department => department.Users)
            .FirstOrDefault(user => user.Id == request.Id);
        if (user is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        if (request.CallsignFormatValues is not null && request.CallsignFormatValues.Count > 0)
        {
            var callsign = string.Join(string.Empty, request.CallsignFormatValues);
            var callsignMatchesWithFormat = !string.IsNullOrEmpty(callsign) && new Regex("^[A-Z0-9-]+$").IsMatch(callsign);
            if (!callsignMatchesWithFormat)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            if (user.Department.Users.Any(x => x.Callsign == callsign))
            {
                networkCallbackDelegate.InvokeWithResponse(false, "UI.ERROR.USER.CALLSIGN.EDIT.TITLE", "UI.ERROR.USER.CALLSIGN.EDIT.DESCRIPTION");
                return;
            }

            user.Callsign = callsign;
            if (user.CallsignFormat is not null)
            {
                user.CallsignFormat.Values = request.CallsignFormatValues;
            }
            else
            {
                var entry = this._databaseContext.UserCallsigns.Add(new UserCallsignFormat
                {
                    Values = request.CallsignFormatValues,
                });
                this._databaseContext.SaveChanges();
                user.UserCallsignFormatId = entry.Entity.Id;
            }
        }

        if (request.PreferredLanguage is not null)
        {
            user.PreferredLanguage = request.PreferredLanguage;
            player.TriggerEvent(Events.UI.Player.LocalizationChange, this._localizationService.GetLocalization(request.PreferredLanguage));
        }

        if (request.AudioVolume is not null)
        {
            user.AudioVolume = request.AudioVolume ?? 0;
        }

        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.CHANGE", "UI.LABEL.PLAYER.SUCCESS.CHANGE_DETAILS");
    }
}