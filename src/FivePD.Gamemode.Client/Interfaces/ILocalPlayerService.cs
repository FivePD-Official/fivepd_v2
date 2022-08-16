// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using CitizenFX.Core;
using FivePD.Common.Models;

namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Provides access to the client's player.
    /// </summary>
    public interface ILocalPlayerService
    {
        /// <summary>
        /// Gets the local <see cref="FPlayer"/>.
        /// </summary>
        public FPlayer Player { get; }
    }
}