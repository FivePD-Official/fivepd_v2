using System.Threading.Tasks;
using FivePD.Server.API;
using FivePD.Server.Interfaces;

namespace FivePD.IA.AmbientEvents;

#pragma warning disable

public class AmbientEvents : IAddon
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