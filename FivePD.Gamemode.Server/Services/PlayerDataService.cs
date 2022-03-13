// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using FivePD.Common.Models;
using FivePD.Gamemode.Server.Interfaces;

namespace FivePD.Gamemode.Server.Services
{
    /**
     * <inheritdoc />
     */
    public class PlayerDataService : IPlayerDataService
    {
        private readonly IServerStateService _serverStateService;

        /**
         * <summary>Initializes a new instance of the <see cref="PlayerDataService"/> class.</summary>
         * <param name="serverStateService">The <see cref="IServerStateService"/> to use.</param>
         */
        public PlayerDataService(IServerStateService serverStateService)
        {
            this._serverStateService = serverStateService;
        }

        /**
         * <inheritdoc />
         */
        public event EventHandler<DutyStatusChangedEventArgs> DutyStatusChanged;

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
    }
}