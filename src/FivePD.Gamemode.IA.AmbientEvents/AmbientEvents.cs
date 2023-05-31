using System.Threading.Tasks;
using FivePD.Gamemode.Server.API;
using FivePD.Gamemode.Server.Interfaces;

namespace FivePD.Gamemode.IA.AmbientEvents;

#pragma warning disable

[AddonMetadata("AmbientEventProvider", "1.0.0", 100)]
public class AmbientEvents : IGamemodeAddon
{
    private readonly IAddonLoggerService _logger;

    public AmbientEvents(IAddonLoggerService logger)
    {
        this._logger = logger;
    }

    public Task OnStartedAsync()
    {
        this._logger.Information("I started!");

        return Task.CompletedTask;
    }

    public Task OnStoppedAsync()
    {
        this._logger.Information("I stopped!");

        return Task.CompletedTask;
    }
}