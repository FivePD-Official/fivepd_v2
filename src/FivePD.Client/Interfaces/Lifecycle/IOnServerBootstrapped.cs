namespace FivePD.Client.Interfaces.Lifecycle;

/// <summary>
/// Services which require the server to be fully bootstrapped
/// before they can perform their initialization should inherit
/// this interface.
/// </summary>
internal interface IOnServerBootstrapped
{
    /// <summary>
    /// Triggered when the server has finished bootstrapping.
    /// </summary>
    void OnServerBootstrapped();
}
