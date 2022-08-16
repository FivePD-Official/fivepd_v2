// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using CitizenFX.Core;
using FivePD.Common.Models;
using FivePD.Gamemode.Client.Interfaces;

namespace FivePD.Gamemode.Client.Services
{
    /// <inheritdoc />
    public class LocalPlayerService : ILocalPlayerService
    {
        /// <inheritdoc />
        public FPlayer Player { get; } = FPlayer.FromNetId(Game.Player.ServerId);
    }
}