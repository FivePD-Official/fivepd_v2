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

public class ActiveShooter : Callout
{
    private readonly IAddonLoggerService _logger;
    private readonly IRandomService _randomService;

    public ActiveShooter(IAddonLoggerService logger, IRandomService randomService)
    {
        this._logger = logger;
        this._randomService = randomService;
    }

    public override Task<CalloutSetupProperties> Setup()
    {
        var suspectCount = this._randomService.Random.Next(1, 5);

        return Task.FromResult(new CalloutSetupProperties
        {
            Title = $"Active shooter{(suspectCount > 1 ? "s" : string.Empty)}",
            Description = suspectCount switch
            {
                1 => "Reports of an active shooter.",
                _ => $"Reports of {suspectCount} active shooters."
            },
            ActivityRadius = 20f,
            RandomPriority = true,
            Locations = new[]
            {
                new Vector3(723.24f, -148.43f, 50.49f),
                new Vector3(-2613.39917f, 1871.93274f, 167.319855f),
                new Vector3(-1026.67212f, 4320.706f, 69.5507f),
                new Vector3(1670.75269f, 3575.47168f, 35.61455f),
                new Vector3(-1003.07983f, 1170.85571f, 218.741821f),
            }
        });
    }

    public override Task<bool> CanBeDispatched()
    {
        return Task.FromResult(true);
    }

    public override Task OnDispatched(List<Unit> units)
    {
        this._logger.Information($"OnDispatched - {units.Count} units");
        return Task.FromResult(0);
    }
    
    public override Task OnDispatched(Unit units)
    {
        this._logger.Information($"OnDispatched - {units.Callsign}");
        return Task.FromResult(0);
    }

    public override Task OnAccepted(Unit unit)
    {
        this._logger.Information($"OnAccepted - {unit.Callsign}");
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