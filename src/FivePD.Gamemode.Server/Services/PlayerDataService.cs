// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.Database;
using FivePD.Gamemode.Server.Exceptions;
using FivePD.Gamemode.Server.Extensions;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models;
using FivePD.Gamemode.Server.Models.Requests;
using FivePD.Gamemode.Server.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace FivePD.Gamemode.Server.Services
{
    /**
     * <inheritdoc />
     */
    public class PlayerDataService : IPlayerDataService, IServiceWithEvents
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly DatabaseContext _databaseContext;
        private readonly PlayerList _playerList;

        /**
         * <summary>Initializes a new instance of the <see cref="PlayerDataService"/> class.</summary>
         * <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
         * <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
         * <param name="playerList">The <see cref="PlayerList"/> to use.</param>
         */
        public PlayerDataService(EventHandlerDictionary eventHandlerDictionary, DatabaseContext databaseContext, PlayerList playerList)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._databaseContext = databaseContext;
            this._playerList = playerList;
            this.RegisterEvents();
        }

        /// <inheritdoc />
        public event EventHandler<Rank> PlayerRankChanged;

        /// <inheritdoc />
        public event EventHandler<Division> PlayerDivisionChanged;

        /// <inheritdoc />
        public void RegisterEvents()
        {
            this._eventHandlerDictionary["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(this.OnPlayerConnecting);
            this._eventHandlerDictionary[Events.Player.GetDetails] += new Action<Player, ExpandoObject, NetworkCallbackDelegate>(this.GetPlayerDetails);
            this._eventHandlerDictionary[Events.Player.ChangeDivision] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.ChangePlayerDivisionFromNui);
            this._eventHandlerDictionary[Events.Player.ChangeRank] += new Action<ExpandoObject, NetworkCallbackDelegate>(this.ChangePlayerRankFromNui);
            this.CreateDataForOnlinePlayers();
        }

        private void CreateDataForOnlinePlayers()
        {
            foreach (var player in this._playerList)
            {
                this.CreatePlayerData(player);
            }
        }

        private void OnPlayerConnecting([FromSource] Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            this.CreatePlayerData(player);
        }

        private void CreatePlayerData(Player player)
        {
            var license = player.Identifiers["license"];
            if (string.IsNullOrEmpty(license))
            {
                Debug.WriteLine($"License of {player.Name} is not defined.");
            }
            else
            {
                try
                {
                    if (this._databaseContext.Users.FirstOrDefault(user => user.License == license) is null)
                    {
                        this._databaseContext.Users.Add(new User
                        {
                            Name = player.Name,
                            License = license,
                        });
                        this._databaseContext.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    throw new DatabaseUsageException("Failed to access database", e);
                }
            }
        }

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
                    .FirstOrDefault(user => user.License == license);
                networkCallbackDelegate.Invoke(user is null ? null : Converter.SerializeObjectWithoutReferenceLoop(user));
            }
            catch (Exception e)
            {
                networkCallbackDelegate.Invoke(null);
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

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
                        Title = "Nui.Errors.UnexpectedError",
                        Description = "Nui.Errors.DatabaseAccess",
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
                            Title = "Nui.Errors.UnexpectedError",
                            Description = "Nui.Errors.DatabaseAccess",
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
                    Title = "Nui.Labels.SuccessfulChange",
                    Description = "Nui.Labels.SuccessfulPlayerDivisionChange",
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
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }

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
                        Title = "Nui.Errors.UnexpectedError",
                        Description = "Nui.Errors.DatabaseAccess",
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
                            Title = "Nui.Errors.UnexpectedError",
                            Description = "Nui.Errors.DatabaseAccess",
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
                    Title = "Nui.Labels.SuccessfulChange",
                    Description = "Nui.Labels.SuccessfulPlayerRankChange",
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
                    Title = "Nui.Errors.UnexpectedError",
                    Description = "Nui.Errors.DatabaseAccess",
                }.ToString());
                throw new DatabaseUsageException("Failed to access database", e);
            }
        }
    }
}