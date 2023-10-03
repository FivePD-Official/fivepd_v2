namespace FivePD.Server.Interfaces.Lifecycle;

/// <summary>
/// Services which require events in order to function properly
/// should inherit this interface.
/// </summary>
public interface IOnInit
{
    /// <summary>
    /// Registers all events related to the current class.
    /// </summary>
    void OnInit();
}