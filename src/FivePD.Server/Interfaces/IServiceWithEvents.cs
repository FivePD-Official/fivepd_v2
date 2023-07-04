namespace FivePD.Server.Interfaces;

/// <summary>
/// Services which require events in order to function properly
/// should inherit this interface.
/// </summary>
public interface IServiceWithEvents
{
    /// <summary>
    /// Registers all events related to the current class.
    /// </summary>
    void RegisterEvents();
}