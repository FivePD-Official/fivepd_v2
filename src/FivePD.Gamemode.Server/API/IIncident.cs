using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CitizenFX.Core;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.Server.API;

#pragma warning disable

public interface IIncident
{
    public Guid Id { get; }

    public string Title { get; }

    public string Description { get; }

    public Vector3 Location { get; }

    public ReadOnlyCollection<Unit> Units { get; }
}

internal interface IInternalIncident
{
    public List<Unit> Units { get; set; }
}