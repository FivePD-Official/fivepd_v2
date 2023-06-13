using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Server.EntityWrappers;

namespace FivePD.Gamemode.Server.API;

#pragma warning disable

public abstract class Callout : IIncident, IInternalIncident
{
    private List<FPlayer> _players = new();

    public Guid Id { get; internal set; }

    public string Title { get; internal set; }

    public FIncidentPriority Priority { get; internal set; }

    public float ActivityRadius { get; internal set; }

    public Vector3 Location { get; internal set; }

    public ReadOnlyCollection<FPlayer> Players => this._players.AsReadOnly();

    public abstract Task<CalloutProperties> Setup();

    public abstract Task<bool> CanBeDispatched();

    public abstract Task OnDispatched(List<FPlayer> dispatchedToPlayers);

    public abstract Task OnAccepted(FPlayer player);

    public abstract Task OnPlayerEnteredInActivityZone(FPlayer player);

    public abstract Task OnCompleted(); // Should be invoked when all players are Code 4 or a player has pressed the "End incident" button

    List<FPlayer> IInternalIncident.Players
    {
        get => this._players;
        set => this._players = value;
    }
}