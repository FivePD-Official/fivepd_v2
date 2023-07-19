using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common.Enums;
using FivePD.Server.API;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;

namespace FivePD.IA.AmbientEvents;

#pragma warning disable

public class KnifeWieldingPerson : Callout
{
    private readonly IAddonLoggerService _logger;

    public KnifeWieldingPerson(IAddonLoggerService logger)
    {
        this._logger = logger;
    }

    public override Task<CalloutSetupProperties> Setup()
    {
        return Task.FromResult(new CalloutSetupProperties
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

    public override Task OnDispatched(List<Unit> notifiedUnits)
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