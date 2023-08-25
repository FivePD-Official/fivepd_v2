namespace FivePD.Server.Interfaces.Addons;

/// <summary>
/// Provides access to basic logging
/// functionality for addon usage.
/// </summary>
public interface IAddonLoggerService
{
    /// <summary>
    /// Logs a message with the information label.
    /// </summary>
    /// <param name="message">The text to write.</param>
    public void Information(string message);
}