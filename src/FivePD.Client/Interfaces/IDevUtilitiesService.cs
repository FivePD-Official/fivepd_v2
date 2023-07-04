namespace FivePD.Client.Interfaces;

/// <summary>
/// Provides functionality related
/// to gamemode development.
/// </summary>
internal interface IDevUtilitiesService
{
    /// <summary>
    /// Prints a development message to
    /// the screen for a brief period of time.
    /// </summary>
    /// <param name="message">The text to show.</param>
    public void ShowDevelopmentMessage(string message);
}