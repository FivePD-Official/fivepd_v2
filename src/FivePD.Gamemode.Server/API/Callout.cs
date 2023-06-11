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
    private List<FPlayer> _players;
    public string Title { get; internal set; }

    public FIncidentPriority Priority { get; internal set; }

    public Vector3 Location { get; internal set; }

    public ReadOnlyCollection<FPlayer> Players { get; }

    public abstract Task<CalloutProperties> Setup();

    public abstract Task<bool> CanBeDispatched();

    public abstract Task OnDispatched(List<FPlayer> dispatchedToPlayers);

    public abstract Task OnAccepted(FPlayer player);

    public abstract Task OnPlayerEnteredInActivityZone(FPlayer player);

    List<FPlayer> IInternalIncident.Players { get; set; }
}