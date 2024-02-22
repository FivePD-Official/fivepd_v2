using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common.Models;
using FivePD.Server.Database;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class LocationService : ILocationService
{
    private readonly PlayerList _playerList;
    private readonly DatabaseContext _databaseContext;
    private readonly IMarkerService _markerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationService"/> class.
    /// Constructs the LocationService.
    /// </summary>
    /// <param name="playerList">A list of players.</param>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    /// <param name="markerService">The <see cref="IMarkerService"/> to use.</param>
    public LocationService(PlayerList playerList, DatabaseContext databaseContext, IMarkerService markerService)
    {
        this._playerList = playerList;
        this._databaseContext = databaseContext;
        this._markerService = markerService;
    }

    /// <inheritdoc />
    public void Update(Player player)
    {
        var markers = this._markerService.GetMarkers().Select(k => k.Key);
        foreach (var guid in markers)
        {
            this._markerService.RemoveMarker(Guid.Parse(guid));
        }

        foreach (var position in this._databaseContext.LocationMarkers.Select(k => k.Position))
        {
            Marker marker = new Marker
            {
                Position = position,
                Color = new Marker.MarkerColor(0, 15, 240, 255),
                TextureDict = null,
                TextureName = null,
            };
            this._markerService.AddMarker(this._playerList, marker);
        }
    }
}