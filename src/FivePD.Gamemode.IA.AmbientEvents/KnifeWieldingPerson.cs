using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Server.API;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models;

namespace FivePD.Gamemode.IA.AmbientEvents;

#pragma warning disable

public class KnifeWieldingPerson : Callout
{
    private readonly IAddonLoggerService _logger;

    public KnifeWieldingPerson(IAddonLoggerService logger)
    {
        this._logger = logger;
    }

    public override Task<CalloutProperties> Setup()
    {
        return Task.FromResult(new CalloutProperties
        {
            Title = "Knife wielding person",
            Description = "A person is wielding a knife.",
            ActivityRadius = 20f,
            Priority = FIncidentPriority.Code3,
            Location = new Vector3(723.24f, -148.43f, 50.49f),
        });
    }

    public override Task<bool> CanBeDispatched()
    {
        return Task.FromResult(true);
    }

    public override Task OnDispatched(List<Unit> dispatchedToUnits)
    {
        this._logger.Information("OnDispatched");

        return Task.FromResult(0);
    }

    public override Task OnAccepted(Unit unit)
    {
        this._logger.Information("OnAccepted");

        return Task.FromResult(0);
    }

    public override Task OnCompleted()
    {
        this._logger.Information($"OnCompleted");

        return Task.FromResult(0);
    }

    public override Task OnPlayerEnteredInActivityZone(FPlayer player)
    {
        this._logger.Information($"OnPlayerEnteredInActivityZone: {player.GetUnderlyingPlayer().Name}");

        return Task.FromResult(0);
    }
}