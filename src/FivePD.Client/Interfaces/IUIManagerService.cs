using Newtonsoft.Json.Linq;

namespace FivePD.Client.Interfaces;

#pragma warning disable

internal interface IUIManagerService
{
    public bool MenuOpen { get; }

    public bool WelcomeModalOpen { get; }

    /// <summary>
    /// Send control event to the NUI.
    /// </summary>
    /// <param name="data">The data to send.</param>
    public void SendMessage(string data);

    /// <summary>
    /// Send control event to the NUI.
    /// </summary>
    /// <param name="data">The data to send.</param>
    public void SendMessage(JObject data);

    /// <summary>
    /// Shows NUI. If the MDT has been opened while any menu is open,
    /// the NUI view will be switched to the MDT.
    /// </summary>
    /// <param name="type">Switches to the given type after showing Nui.</param>
    /// <param name="menuId">The currently opened menu's id. If left as # it'll be ignored in the Nui.</param>
    public void ToggleView(string type, string menuId = "");

    /// <summary>
    /// Hides NUI.
    /// </summary>
    public void Hide();

    /// <summary>
    /// Closes the NUI.
    /// </summary>
    public void Close();

    /// <summary>
    /// Displays a timer bar.
    /// </summary>
    /// <param name="timeToHoldInMs">Determines for how long the key should be pressed to trigger the action.</param>
    public void ShowTimerBar(int timeToHoldInMs);

    /// <summary>
    /// Updates the progress of the timer bar.
    /// </summary>
    /// <param name="currentProgressMs">The current elapsed time in milliseconds.</param>
    /// <param name="totalDurationMs">The total duration in milliseconds.</param>
    public void UpdateTimerBarProgress(int currentProgressMs, int totalDurationMs);
}
