using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Exceptions;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.DTO;
using FivePD.Server.Models.Events;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.IPlayerDataService" />
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
    public event EventHandler<PlayerRankChangeEventArgs> PlayerRankChanged;

    /// <inheritdoc />
    public event EventHandler<PlayerDivisionChangeEventArgs> PlayerDivisionChanged;

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
        catch (Exception)
        {
            return null;
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

    /// <inheritdoc />
    public void IncreaseRank(FPlayer player)
    {
        if (!this._configService.GeneralConfig.FeatureFlags.ProgressionSystem)
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
        this._databaseContext
            .Entry(user.Department)
            .Collection(x => x.Ranks)
            .Load();
        var ranks = user.Department.Ranks.OrderBy(x => x.RequiredXp).ToList();
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
        if (!this._configService.GeneralConfig.FeatureFlags.ProgressionSystem)
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
        this._databaseContext
            .Entry(user.Department)
            .Collection(x => x.Ranks)
            .Load();
        var ranks = user.Department.Ranks.OrderBy(x => x.RequiredXp).ToList();
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
        catch (Exception)
        {
            networkCallbackDelegate.Invoke(null);
        }
    }

    [UIEvent(Events.UI.Player.ChangeDivision, true)]
    private void ChangePlayerDivisionFromNui([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
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
            this.PlayerDivisionChanged?.Invoke(this, new PlayerDivisionChangeEventArgs
            {
                Division = division,
                Player = FPlayer.FromPlayer(player),
            });
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
        catch (Exception)
        {
            networkCallbackDelegate.Invoke(new ActionResponse
            {
                Status = false,
                Title = "UI.ERROR.UNEXPECTED",
                Description = "UI.ERROR.DATABASE_ACCESS.TITLE",
            }.ToString());
        }
    }

    [UIEvent(Events.UI.Player.ChangeRank, true)]
    private void ChangePlayerRankFromNui([FromSource] Player player, ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
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
        catch (Exception)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
        }
    }

    [UIEvent(Events.UI.Player.ChangeFixedRank)]
    private void ChangePlayerFixedRankFromNui(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        try
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
            networkCallbackDelegate.Invoke(new ActionResponse
            {
                Status = true,
                Title = "UI.LABEL.SUCCESS.CHANGE",
                Description = request.FixedRank ? "UI.LABEL.PLAYER.SUCCESS.CHANGE_FIXED_RANK" : "UI.LABEL.PLAYER.SUCCESS.CHANGE_NOT_FIXED_RANK",
                Params = new Dictionary<string, string>
                {
                    { "name", user.Name },
                },
            }.ToString());
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            networkCallbackDelegate.InvokeWithUnexpectedError();
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