using System.Collections.Generic;
using System.Collections.ObjectModel;
using CitizenFX.Core;
using FivePD.Gamemode.Server.EntityWrappers;

namespace FivePD.Gamemode.Server.API;

#pragma warning disable

public interface IIncident
{
    public string Title { get; }

    public Vector3 Location { get; }

    public ReadOnlyCollection<FPlayer> Players { get; }
}

internal interface IInternalIncident
{
    public List<FPlayer> Players { get; set; }
}