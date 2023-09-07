using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Client.Interfaces;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Common.Models;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IMarkerService" />
internal class MarkerService : IMarkerService, IServiceWithEvents, IDisposableService
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    private readonly ITickService _tickService;

    private readonly Dictionary<string, Marker> _markersToDraw = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkerService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    /// <param name="tickService">The <see cref="ITickService"/> to use.</param>
    public MarkerService(EventHandlerDictionary eventHandlerDictionary, ITickService tickService)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
        this._tickService = tickService;
    }

    /// <inheritdoc />
    public void RegisterEvents()
    {
        this._eventHandlerDictionary[Events.Marker.Add] += new Action<string>(this.AddMarker);
        this._eventHandlerDictionary[Events.Marker.Remove] += new Action<string>(this.RemoveMarker);
        this._tickService.OnTick += this.DrawMarkersAsync;
    }

    /// <inheritdoc/>
    public void InternalDispose()
    {
        this._tickService.OnTick -= this.DrawMarkersAsync;
    }

    private void AddMarker(string serializedRequest)
    {
        var request = JsonConvert.DeserializeObject<MarkerDTO.Add>(serializedRequest);
        this._markersToDraw.Add(request.Guid, request.Marker);
    }

    private void RemoveMarker(string id)
    {
        this._markersToDraw.Remove(id);
    }

    private Task DrawMarkersAsync()
    {
        foreach (var marker in this._markersToDraw.Values)
        {
            Cfx.API.DrawMarker(
                (int)marker.Type,
                marker.Position.X,
                marker.Position.Y,
                marker.Position.Z,
                marker.Direction.X,
                marker.Direction.Y,
                marker.Direction.Z,
                marker.Rotation.X,
                marker.Rotation.Z,
                marker.Rotation.Z,
                marker.Scale.X,
                marker.Scale.Y,
                marker.Scale.Z,
                marker.Color.Red,
                marker.Color.Green,
                marker.Color.Blue,
                marker.Color.Alpha,
                marker.BobUpAndDown,
                marker.FaceCamera,
                marker.P19,
                marker.Rotate,
                marker.TextureDict,
                marker.TextureName,
                marker.DrawOnEnts);
        }

        return Task.FromResult(0);
    }
}