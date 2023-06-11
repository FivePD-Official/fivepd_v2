using System.Collections.Generic;
using System.Threading.Tasks;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Server.API;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Interfaces;

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
            ActivityRadius = 20f,
            Priority = FIncidentPriority.Code3,
        });
    }

    public override Task<bool> CanBeDispatched()
    {
        return Task.FromResult(true);
    }

    public override Task OnDispatched(List<FPlayer> dispatchedToPlayers)
    {
        this._logger.Information("OnDispatched");

        return Task.FromResult(0);
    }

    public override Task OnAccepted(FPlayer player)
    {
        this._logger.Information("OnAccepted");

        return Task.FromResult(0);
    }

    public override Task OnPlayerEnteredInActivityZone(FPlayer player)
    {
        this._logger.Information("OnPlayerEnteredInActivityZone");

        return Task.FromResult(0);
    }
}