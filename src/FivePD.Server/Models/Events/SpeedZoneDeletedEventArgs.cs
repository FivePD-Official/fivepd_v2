using System;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;

namespace FivePD.Server.Models.Events;

/// <summary>
/// Provides additional information regarding a player's
/// department change, both when they join or leave one.
/// </summary>
/// <param name="Player">The player who deleted the speed zone. If it has been created using the API it will be null.</param>
/// <param name="Id">The id of the speed zone.</param>
/// <param name="SpeedZone">The speed zone.</param>
public sealed record SpeedZoneDeletedEventArgs(FPlayer Player, Guid Id, SpeedZone SpeedZone);