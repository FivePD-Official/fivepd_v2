using System.Threading.Tasks;

namespace FivePD.Gamemode.API
{
    /**
     * <summary>Represents a server-sided gamemode addon.</summary>
     */
    public interface IGamemodeAddon
    {
        /**
         * <summary>Executed when the addon is loaded by the GAR.</summary>
         * <returns>A task which should complete when the addon is ready.</returns>
         */
        public Task OnStarted();

        /**
         * <summary>Executed when the GAR has requested the addon to stop.</summary>
         * <returns>A task which should complete when the addon is ready.</returns>
         */
        public Task OnStopped();
    }
}