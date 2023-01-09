namespace FivePD.Gamemode.Client.Interfaces
{
    /// <summary>
    /// Responsible for handling traffic stops.
    /// </summary>
    public interface ITrafficStopService
    {
        /// <summary>
        /// Registers all traffic stop related keybinds.
        /// </summary>
        public void RegisterKeybinds();
    }
}