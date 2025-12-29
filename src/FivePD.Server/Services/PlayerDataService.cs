using System;
using System.Collections;
using System.Collections.Generic;
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
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Events;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using FivePD.Server.Repositories;
using Newtonsoft.Json;
using Serilog;

namespace FivePD.Server.Services;

#pragma warning disable

/// <inheritdoc cref="FivePD.Server.Interfaces.IPlayerDataService" />
[Obfuscate]
public class PlayerDataService : IPlayerDataService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    private readonly PlayerList _playerList;

    private readonly IGameNotificationService _notificationService;
    private readonly IConfigService _configService;

    private readonly ILogger _logger;

    private readonly ILocalizationService _localizationService;
    private readonly IPlayerHandlerService _playerHandlerService;

    // private readonly IInternalPedStorageService _internalPedStorageService;
    private readonly INetworkEventService _networkEventService;
    private readonly UserRepository _userRepository;
    private readonly RankRepository _rankRepository;
    private readonly DepartmentRepository _departmentRepository;
    private readonly DivisionRepository _divisionRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerDataService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="playerList">The <see cref="PlayerList"/> to use.</param>
    /// <param name="notificationService">The <see cref="IGameNotificationService"/> to use.</param>
    /// <param name="configService">The <see cref="IConfigService"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    /// <param name="localizationService">The <see cref="ILocalizationService"/> to use.</param>
    /// <param name="playerHandlerService">The <see cref="IPlayerHandlerService"/> to use.</param>
    /// <param name="pedStorageService">The <see cref="IPedStorageService"/> to use.</param>
    /// <param name="networkEventService">The <see cref="INetworkEventService"/> to use.</param>
    /// <param name="userRepository">The <see cref="UserRepository"/> to use.</param>
    /// <param name="rankRepository">The <see cref="RankRepository"/> to use.</param>
    /// <param name="departmentRepository">The <see cref="DepartmentRepository"/> to use.</param>
    /// <param name="divisionRepository">The <see cref="DivisionRepository"/> to use.</param>
    public PlayerDataService(
        EventHandlerDictionary eventHandlerDictionary,
        PlayerList playerList,
        IGameNotificationService notificationService,
        IConfigService configService,
        ILogger logger,
        ILocalizationService localizationService,
        IPlayerHandlerService playerHandlerService,
        // IPedStorageService pedStorageService,
        INetworkEventService networkEventService,
        UserRepository userRepository,
        RankRepository rankRepository,
        DepartmentRepository departmentRepository,
        DivisionRepository divisionRepository)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._playerList = playerList;
        this._notificationService = notificationService;
        this._configService = configService;
        this._logger = logger;
        this._localizationService = localizationService;
        this._playerHandlerService = playerHandlerService;
        // this._internalPedStorageService = (IInternalPedStorageService)pedStorageService;
        this._networkEventService = networkEventService;
        this._userRepository = userRepository;
        this._rankRepository = rankRepository;
        this._departmentRepository = departmentRepository;
        this._divisionRepository = divisionRepository;
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
        this._eventHandlerDictionary[Events.Player.OnConnected] += new Action<Player, bool>(this.OnPlayerConnected);
        this._eventHandlerDictionary[Events.Player.GetDismissedWelcomeModal] += new Action<Player, string>(this.GetDismissedWelcomeModal);
        this._eventHandlerDictionary[Events.Player.DismissedWelcomeModal] += new Action<Player>(this.DismissedWelcomeModal);
        this._eventHandlerDictionary[Events.Player.GetRole] += new Action<Player, string>(this.GetRole);
    }

    /// <inheritdoc/>
    public User FindUserByLicense(Player player, bool withChangeTracking = true)
    {
        return this._userRepository.FindByLicense(player);
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

        var ranksToUse = user.Department.UseGlobalRanks ? this._rankRepository.GetAllGlobal() : this._departmentRepository.GetRanks(user.Department);
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

        this._userRepository.Update(user, x => { x.RankId = nextRank.Id; });
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

        var ranksToUse = user.Department.UseGlobalRanks ? this._rankRepository.GetAllGlobal() : this._departmentRepository.GetRanks(user.Department);
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

        this._userRepository.Update(user, x => { x.RankId = previousRank.Id; });
        this.PlayerRankChanged?.Invoke(this, new PlayerRankChangeEventArgs
        {
            Rank = previousRank,
            Player = player,
        });
    }

    private void OnPlayerConnected([FromSource] Player player, bool invokeServerBootstrappedEvent)
    {
        if (invokeServerBootstrappedEvent)
        {
            player.TriggerEvent(Events.Lifecycle.ServerBootstrapped);
            return;
        }

        this.CreatePlayerData(player);
        this.SubscribeToOnDutyStatusChange(player);
        // this._internalPedStorageService.InternalPeds[player.Character.NetworkId] = new FPed(new FPedBuilder(new CommonFPedBuilder(player.Character.NetworkId)));
    }

    private void SubscribeToOnDutyStatusChange(Player player)
    {
        var fPlayer = FPlayer.FromNetId(player.Handle);
        fPlayer.DutyStatus.Subscribe(status => this.OnPlayerDutyStatusChanged(player, status), false);
    }

    private void OnPlayerDutyStatusChanged(Player player, FDutyStatus status)
    {
        var fPlayer = FPlayer.FromNetId(player.Handle);
        if (fPlayer is null)
        {
            return;
        }

        this.PlayerDutyStatusChanged?.Invoke(
            this,
            new PlayerDutyStatusChangeEventArgs
            {
                Player = fPlayer,
                Status = status,
            });

        if (!this._configService.GeneralConfig.Features.DutyNotification)
        {
            return;
        }

        foreach (var p in this._playerList.Where(p => p.Handle != player.Handle))
        {
            this._notificationService.Notify(
                p,
                new GameNotificationOptions
                {
                    Title = "LABEL.DISPATCH",
                    Message = "LABEL.PLAYER_STATUS.CHANGE",
                    MessageParams = new Dictionary<string, string>
                    {
                        ["name"] = player.Name,
                        ["status"] = status switch
                        {
                            FDutyStatus.OffDuty => "LABEL.PLAYER_STATUS.OFF_DUTY",
                            FDutyStatus.OnDuty => "LABEL.PLAYER_STATUS.ON_DUTY",
                            _ => "Unknown",
                        },
                    },
                });
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

        var userCount = this._userRepository.Count();
        var existingUser = this._userRepository.FindByLicense(player);
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

            this._userRepository.Add(user);
        }
        else
        {
            if (!existingUser.FirstLogin)
            {
                return;
            }

            this._userRepository.Update(existingUser, x => { x.FirstLogin = false; });
        }
    }

    private void GetRole([FromSource] Player player, string guid)
    {
        var user = this._playerHandlerService.FindUserOfPlayer(player);
        this._networkEventService.TriggerEvent(player, guid, user?.Role ?? 0);
    }

    [UIEvent(Events.UI.Player.GetDetails)]
    private void GetPlayerDetailsUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var user = this._userRepository.FindByLicense(player);
        if (user is null)
        {
            this._networkEventService.TriggerEventSerialized(player, guid, null);
            return;
        }
        
        var fPlayer = FPlayer.FromNetId(player.Handle);
        // var unit = fPlayer.Unit;
        
        var playerDetails = new PlayerDetailsDTO
        {
            NetworkId = player.Handle,
            User = user,
            DutyStatus = fPlayer.DutyStatus.Value,
            // UnitCallsign = unit is null ? string.Empty : unit.Callsign,
            UnitCallsign =  null,
        };
        this._networkEventService.TriggerEventSerialized(player, guid, playerDetails);
    }

    private void GetDismissedWelcomeModal([FromSource] Player player, string guid)
    {
        var user = this._userRepository.FindByLicense(player);
        if (user is null)
        {
            this._networkEventService.TriggerEvent(player, guid, false);
            return;
        }

        this._networkEventService.TriggerEvent(player, guid, user.DismissedWelcomeMessage);
    }

    private void DismissedWelcomeModal([FromSource] Player player)
    {
        var user = this._userRepository.FindByLicense(player);
        if (user is null)
        {
            return;
        }

        this._userRepository.Update(user, x => { x.DismissedWelcomeMessage = true; });
    }

    [UIEvent(Events.UI.Player.ChangeDivision)]
    private void ChangePlayerDivisionFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<PlayerChangeRequest>(serializedRequest);
        var user = this._userRepository.FindById(request.UserId);
        if (user is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        Division division;
        if (request.IdToChange == -1)
        {
            division = null;
            this._userRepository.Update(user, x => { x.DivisionId = 0; });
        }
        else
        {
            division = this._divisionRepository.FindById(request.IdToChange);
            if (division is null)
            {
                this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
                return;
            }

            this._userRepository.Update(user, x => { x.DivisionId = request.IdToChange; });
        }

        this.PlayerDivisionChanged?.Invoke(this, new PlayerDivisionChangeEventArgs
        {
            Division = division,
            Player = FPlayer.FromPlayer(player),
        });
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.CHANGE",
            "LABEL.DIVISION.SUCCESS.CHANGE",
            new Dictionary<string, string>
            {
                { "player", user.Name },
            });
    }

    [UIEvent(Events.UI.Player.ChangeRank)]
    private void ChangePlayerRankFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<PlayerChangeRequest>(serializedRequest);
        var user = this._userRepository.FindById(request.UserId);
        if (user is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        Rank rank;
        if (request.IdToChange == -1)
        {
            rank = null;
            this._userRepository.Update(user, x => { x.RankId = 0; });
        }
        else
        {
            rank = this._rankRepository.FindById(request.IdToChange);
            if (rank is null)
            {
                this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
                return;
            }

            this._userRepository.Update(user, x => { x.RankId = request.IdToChange; });
        }

        this.PlayerRankChanged?.Invoke(this, new PlayerRankChangeEventArgs
        {
            Rank = rank,
            Player = FPlayer.FromPlayer(player),
        });
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.CHANGE",
            "LABEL.RANK.SUCCESS.CHANGE",
            new Dictionary<string, string>
            {
                { "player", user.Name },
            });
    }

    [UIEvent(Events.UI.Player.ChangeFixedRank)]
    private void ChangePlayerFixedRankFromNui([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<PlayerChangeFixedRankRequest>(serializedRequest);
        var user = this._userRepository.FindById(request.UserId);
        if (user is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._userRepository.Update(user, x => { x.FixedRank = request.FixedRank; });
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.CHANGE",
            request.FixedRank ? "LABEL.PLAYER.SUCCESS.CHANGE_FIXED_RANK" : "LABEL.PLAYER.SUCCESS.CHANGE_NOT_FIXED_RANK",
            new Dictionary<string, string>
            {
                { "name", user.Name },
            });
    }

    [UIEvent(Events.UI.Player.GetAllPageable)]
    private void GetAllPageable([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<GetSetRequest>(serializedRequest);
        var page = this._userRepository.Paginate(request.Page, new SqlBuilder());
        this._networkEventService.TriggerEventSerialized(player, guid, page);
    }

    [UIEvent(Events.UI.Player.ChangeRole)]
    private void ChangeRole([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<ChangePlayerRoleRequest>(serializedRequest);
        var user = this._userRepository.FindById(request.UserId);
        if (user is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        this._userRepository.Update(user, x => { user.Role = request.Roles.Sum(); });
        this._networkEventService.TriggerEventWithResponse(
            player,
            guid,
            true,
            "LABEL.SUCCESS.CHANGE",
            "LABEL.PLAYER.SUCCESS.CHANGE_ROLE",
            new Dictionary<string, string>
            {
                { "name", user.Name },
            });

        var playerOfUser = this._playerHandlerService.FindPlayerOfUser(user);
        playerOfUser?.TriggerEvent(Events.Player.OnRoleChange, user.Role);
    }

    [UIEvent(Events.User.GetAllSimple)]
    private void GetAllSimple([FromSource] Player player, string guid, string serializedRequest)
    {
        var users = this._userRepository
            .GetAll()
            .Select(x => new SimpleUserDTO
            {
                Id = x.Id,
                Name = x.Name,
            });
        this._networkEventService.TriggerEventSerialized(player, guid, users);
    }

    [UIEvent(Events.UI.Player.UpdateDetails)]
    private void UpdatePlayerDetailsUI([FromSource] Player player, string guid, string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<UpdatePlayerDetailsRequest>(serializedRequest);
        var user = this._userRepository.FindById(request.Id);
        if (user is null)
        {
            this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
            return;
        }

        var departmentUsers = user.DepartmentId is null ? [] : this._departmentRepository.GetUsersById(user.DepartmentId ?? 0);

        if (request.CallsignFormatValues is not null && request.CallsignFormatValues.Count > 0)
        {
            var callsign = string.Join(string.Empty, request.CallsignFormatValues);
            var callsignMatchesWithFormat = !string.IsNullOrEmpty(callsign) && new Regex("^[A-Z0-9-]+$").IsMatch(callsign);
            if (!callsignMatchesWithFormat)
            {
                this._networkEventService.TriggerEventWithUnexpectedError(player, guid);
                return;
            }

            if (departmentUsers.Any(x => x.Callsign == callsign))
            {
                this._networkEventService.TriggerEventWithResponse(
                    player,
                    guid,
                    false,
                    "ERROR.USER.CALLSIGN.EDIT.TITLE",
                    "ERROR.USER.CALLSIGN.EDIT.DESCRIPTION");
                return;
            }

            this._userRepository.Update(user, x => { x.Callsign = callsign; });
            if (user.CallsignFormat is not null)
            {
                var userCallsignFormat = this._userRepository.GetCallsignFormat(user);
                this._userRepository.UpdateCallsignFormat(userCallsignFormat, x => { x.Values = request.CallsignFormatValues; });
            }
            else
            {
                var userCallsignFormat = this._userRepository.AddCallsignFormat(user, new UserCallsignFormat { Values = request.CallsignFormatValues });
                this._userRepository.Update(user, x => { x.UserCallsignFormatId = userCallsignFormat.Id; });
            }
        }

        if (request.PreferredLanguage is not null)
        {
            this._userRepository.Update(user, x => { x.PreferredLanguage = request.PreferredLanguage; });
            player.TriggerEvent(Events.UI.Player.LocalizationChange, this._localizationService.GetLocalization(request.PreferredLanguage));
        }

        if (request.AudioVolume is not null)
        {
            this._userRepository.Update(user, x => { x.AudioVolume = request.AudioVolume ?? 0; });
        }

        this._networkEventService.TriggerEventWithResponse(player, guid, true, "LABEL.SUCCESS.CHANGE", "LABEL.PLAYER.SUCCESS.CHANGE_DETAILS");
    }
}