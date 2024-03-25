using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;

namespace FivePD.Server.Services;

/// <inheritdoc />
public class LocationService : ILocationService
{
    private readonly PlayerList _playerList;
    private readonly DatabaseContext _databaseContext;
    private readonly IMarkerService _markerService;

    private readonly List<Guid> _markers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LocationService"/> class.
    /// </summary>
    /// <param name="playerList">The <see cref="PlayerList"/> to use..</param>
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
        foreach (var guid in this._markers)
        {
            this._markerService.RemoveMarker(guid);
        }

        this._markers.Clear();

        foreach (var position in this._databaseContext.Locations.Select(k => k.Position))
        {
            var marker = new Marker
            {
                Position = position,
                Color = new Marker.MarkerColor(0, 15, 240, 255),
                TextureDict = null,
                TextureName = null,
            };
            var guid = this._markerService.AddMarker(this._playerList, marker);
            this._markers.Add(guid);
        }
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
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.CREATE", "UI.LABEL.VEHICLE_CONFIG.SUCCESS.CREATE");
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
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.DELETE", "UI.LABEL.VEHICLE_CONFIG.SUCCESS.DELETE");
    }
}