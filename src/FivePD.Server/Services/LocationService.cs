using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Lifecycle;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FivePD.Server.Services;

/// <inheritdoc cref="FivePD.Server.Interfaces.ILocationService" />
public class LocationService : ILocationService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly PlayerList _playerList;
    private readonly DatabaseContext _databaseContext;
    private readonly IMarkerService _markerService;

    private readonly List<Guid> _markers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="playerList">The <see cref="PlayerList"/> to use..</param>
    /// <param name="databaseContext">The <see cref="DatabaseContext"/> to use.</param>
    /// <param name="markerService">The <see cref="IMarkerService"/> to use.</param>
    public LocationService(
        EventHandlerDictionary eventHandlerDictionary,
        PlayerList playerList,
        DatabaseContext databaseContext,
        IMarkerService markerService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._playerList = playerList;
        this._databaseContext = databaseContext;
        this._markerService = markerService;
    }

    /// <inheritdoc />
    void IOnInit.OnInit()
    {
        this._eventHandlerDictionary[Events.Player.OnConnected] += new Action<Player>(this.OnPlayerConnected);

        var locations = this._databaseContext.Locations.ToList();
        foreach (var player in this._playerList)
        {
            this.OnInitOrOnPlayerConnected(player, locations);
        }
    }

    private void OnPlayerConnected([FromSource] Player player)
    {
        this.OnInitOrOnPlayerConnected(player, this._databaseContext.Locations.ToList());
    }

    private void OnInitOrOnPlayerConnected(Player player, List<Location> locations)
    {
        this.UpdateMarkersForPlayer(player, locations);
    }

    [UIEvent(Events.UI.Location.Create)]
    private void CreateLocationUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<CreateLocationRequest>();
        this._databaseContext.Locations.Add(new Location
        {
            Name = request.Name,
            Type = request.Type,
            Position = request.Position,
        });
        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.CREATE", "UI.LABEL.LOCATION.SUCCESS.CREATE");
        this.UpdateMarkersForAllPlayers();
    }

    [UIEvent(Events.UI.Location.GetAllPageable)]
    private void GetAllPageableUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<GetLocationRequest>();
        var filters = new Filters<Location>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Name),
            location => location.Name.ToLower().Contains(request.Filters.Name.ToLower()));
        filters.Add(request.Filters.Type != -1, location => location.Type == (LocationType)request.Filters.Type);
        var page = this._databaseContext.Locations
            .AsNoTracking()
            .Paginate(request.Page, filters);

        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.Location.Delete)]
    private void DeleteLocationUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var entityToRemove = this._databaseContext.Locations.FirstOrDefault(x => x.Id == request.Id);
        if (entityToRemove is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        this._databaseContext.Locations.Remove(entityToRemove);
        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.DELETE", "UI.LABEL.LOCATION.SUCCESS.DELETE");
        this.UpdateMarkersForAllPlayers();
    }

    private void UpdateMarkersForAllPlayers()
    {
        foreach (var guid in this._markers)
        {
            this._markerService.RemoveMarker(guid);
        }

        this._markers.Clear();

        var locations = this._databaseContext.Locations.ToList();
        foreach (var player in this._playerList)
        {
            this.UpdateMarkersForPlayer(player, locations);
        }
    }

    private void UpdateMarkersForPlayer(Player player, List<Location> locations)
    {
        foreach (var location in locations)
        {
            var guid = this._markerService.AddMarker(player, this.CreateMarker(location));
            this._markers.Add(guid);
        }

        player.TriggerEvent(Events.Location.Update, JsonConvert.SerializeObject(locations));
    }

    private Marker CreateMarker(Location location)
    {
        var marker = new Marker
        {
            Type = FMarkerType.MarkerTypeVerticalCylinder,
            Position = location.Position,
            Color = location.Type switch
            {
                LocationType.MdtAccess => new Marker.MarkerColor(0, 0, 255, 255),
                LocationType.JailDropOff => new Marker.MarkerColor(0, 255, 255, 255),
                LocationType.LoadoutSelector => new Marker.MarkerColor(255, 0, 0, 255),
                LocationType.VehicleSelector => new Marker.MarkerColor(0, 255, 0, 255),
                _ => new Marker.MarkerColor(255, 255, 255, 255),
            },
            Scale = new Vector3(1f, 1f, 1f),
        };
        return marker;
    }
}