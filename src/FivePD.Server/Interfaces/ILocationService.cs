using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Server.Migrations;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Provides access to location-related data.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Updates a player's LocationMarker.
    /// </summary>
    /// <param name="player">The specified player.</param>
    public void Update(Player player);
}