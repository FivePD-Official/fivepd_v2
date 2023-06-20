using FivePD.Gamemode.Client.Services;
using Newtonsoft.Json.Linq;

namespace FivePD.Gamemode.Client.Interfaces;

/// <summary>
/// Provides the necessary methods to toggle the NUI
/// and to switch between MDT views and menus.
/// </summary>
internal interface IUIService
{
    /// <summary>
    /// Opens the test radial menu.
    /// </summary>
    /// <param name="menuId">Identifier of the menu.</param>
    public void ShowRadialMenu(string menuId);

    /// <summary>
    /// Opens the MDT.
    /// </summary>
    public void OpenMDT();

    /// <summary>
    /// Opens the information panel.
    /// </summary>
    public void OpenInformation();

    /// <summary>
    /// Moves up by one item in the currently opened menu.
    /// </summary>
    public void MenuControlUp();

    /// <summary>
    /// Moves down by one item in the currently opened menu.
    /// </summary>
    public void MenuControlDown();

    /// <summary>
    /// Moves left by one item in the currently opened menu.
    /// </summary>
    public void MenuControlLeft();

    /// <summary>
    /// Moves right by one item in the currently opened menu.
    /// </summary>
    public void MenuControlRight();

    /// <summary>
    /// Selects the current item in the menu.
    /// </summary>
    public void MenuControlSelect();
}