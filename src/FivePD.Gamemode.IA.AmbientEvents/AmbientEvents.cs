using System.Threading.Tasks;
using FivePD.Gamemode.Server.API;
using FivePD.Gamemode.Server.Interfaces;

namespace FivePD.Gamemode.IA.AmbientEvents
{
    /// <summary>
    /// Responsible for creating and management various ambient
    /// events around players (e.x.: carjackings or speeders).
    /// </summary>
    [AddonMetadata("AmbientEventProvider", "1.0.0", 100)]
    public class AmbientEvents : IGamemodeAddon
    {
        private readonly IAddonLoggerService _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbientEvents"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="IAddonLoggerService"/> to use.</param>
        public AmbientEvents(IAddonLoggerService logger)
        {
            this._logger = logger;
        }

        /// <inheritdoc/>
        public Task OnStartedAsync()
        {
            this._logger.Information("I started!");

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OnStoppedAsync()
        {
            this._logger.Information("I stopped!");

            return Task.CompletedTask;
        }
    }
}