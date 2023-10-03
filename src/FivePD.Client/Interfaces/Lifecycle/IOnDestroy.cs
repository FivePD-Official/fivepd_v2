namespace FivePD.Client.Interfaces.Lifecycle;

/// <summary>
/// Services which require events in order to function properly
/// should inherit this interface.
/// </summary>
internal interface IOnDestroy
{
    /// <summary>
    /// Registers all events related to the current class.
    /// </summary>
    void OnDestroy();
}