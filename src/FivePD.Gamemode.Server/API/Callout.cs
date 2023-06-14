using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.API;

#pragma warning disable

public abstract class Callout : IIncident, IInternalIncident
{
    private List<Unit> _units = new();

    public Guid Id { get; internal set; }

    public string Title { get; internal set; }

    public string Description { get; internal set; }

    public FIncidentPriority Priority { get; internal set; }

    public float ActivityRadius { get; internal set; }

    public Vector3 Location { get; internal set; }

    public ReadOnlyCollection<Unit> Units => this._units.AsReadOnly();

    public abstract Task<CalloutProperties> Setup();

    public abstract Task<bool> CanBeDispatched();

    public abstract Task OnDispatched(List<Unit> dispatchedToUnits);

    public abstract Task OnAccepted(Unit unit);

    public abstract Task OnPlayerEnteredInActivityZone(FPlayer player);

    public abstract Task OnCompleted(); // Should be invoked when all players are Code 4 or a player has pressed the "End incident" button

    List<Unit> IInternalIncident.Units
    {
        get => this._units;
        set => this._units = value;
    }
}