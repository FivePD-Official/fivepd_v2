namespace FivePD.Client.Interfaces;

/// <summary>
/// Services which require events in order to function properly
/// should inherit this interface.
/// </summary>
internal interface IDisposableService
{
    /// <summary>
    /// Registers all events related to the current class.
    /// </summary>
    internal void InternalDispose();
}