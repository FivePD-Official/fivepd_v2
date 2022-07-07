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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FivePD.Gamemode.Server.Services
{
    /**
     * <inheritdoc />
     */
    public class PlayerDataService : IPlayerDataService
    {
        private readonly EventHandlerDictionary _eventHandlerDictionary;
        private readonly IServerStateService _serverStateService;
        private readonly DatabaseContext _databaseContext;
        private readonly PlayerList _playerList;

        /**
         * <summary>Initializes a new instance of the <see cref="PlayerDataService"/> class.</summary>
         * <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
         * <param name="serverStateService">The <see cref="IServerStateService"/> to use.</param>
         * <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
         * <param name="playerList">The <see cref="PlayerList"/> to use.</param>
         */
        public PlayerDataService(EventHandlerDictionary eventHandlerDictionary, IServerStateService serverStateService, DatabaseContext databaseContext, PlayerList playerList)
        {
            this._eventHandlerDictionary = eventHandlerDictionary;
            this._serverStateService = serverStateService;
            this._databaseContext = databaseContext;
            this._playerList = playerList;
        }

        /**
         * <inheritdoc />
         */
        public event EventHandler<DutyStatusChangedEventArgs> DutyStatusChanged;

        /// <inheritdoc />
        public void RegisterEvents()
        {
            this._eventHandlerDictionary["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(this.OnPlayerConnecting);
            this._eventHandlerDictionary[Events.Player.GetDetails] += new Action<Player, ExpandoObject, NetworkCallbackDelegate>(this.GetPlayerDetails);
        }

        /**
         * <inheritdoc />
         */
        public FDutyStatus GetDutyStatus(FPlayer player)
        {
            return this._serverStateService.GetOrAdd<FDutyStatus>(GetStorageSlug(player), "dutyStatus", FDutyStatus.OffDuty);
        }

        /**
         * <inheritdoc />
         */
        public void SetDutyStatus(FPlayer player, FDutyStatus dutyStatus)
        {
            this._serverStateService.Set(GetStorageSlug(player), "dutyStatus", dutyStatus);

            this.DutyStatusChanged?.Invoke(this, new DutyStatusChangedEventArgs()
            {
                Player = player,
                DutyStatus = dutyStatus,
            });
        }

        private static string GetStorageSlug(FPlayer player)
        {
            return $"player:{player.NetId}";
        }

        private void OnPlayerConnecting([FromSource] Player player, string playerName, dynamic setKickReason, dynamic deferrals)
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
    }
}