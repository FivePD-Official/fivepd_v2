namespace FivePD.Gamemode.Server.Interfaces
{
    /**
     * <summary>Provides utilities for development. These
     * methods should only be called in non-prod servers.</summary>
     */
    public interface IDevUtilitiesService
    {
        /**
         * <summary>Registers commands intended for development
         * usage.</summary>
         */
        void RegisterDevelopmentCommands();
    }
}