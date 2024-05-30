namespace FivePD.Client.Interfaces;

/// <summary>
/// Handles all Nui menu related events.
/// </summary>
internal interface IMenuService
{
    /// <summary>
    /// Registers all menus from the server.
    /// </summary>
    public void RegisterMenus();
}