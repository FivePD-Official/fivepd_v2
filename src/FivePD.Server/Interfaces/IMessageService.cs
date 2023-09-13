using CitizenFX.Core;
using FivePD.Server.Models;

namespace FivePD.Server.Interfaces;

/// <summary>
/// Responsible for text communication between players.
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// Sends a message to a player.
    /// </summary>
    /// <param name="to">The user to send the message to.</param>
    /// <param name="subject">The subject of the message.</param>
    /// <param name="content">The content of the message.</param>
    /// <param name="sender">The sender of the message. If left empty the addon's assembly name will be used.</param>
    /// <returns>The <see cref="Message"/> that was sent.</returns>
    public Message Send(User to, string subject, string content, string sender = "");

    /// <summary>
    /// Sends a message to a player.
    /// </summary>
    /// <param name="to">The user to send the message to.</param>
    /// <param name="subject">The subject of the message.</param>
    /// <param name="content">The content of the message.</param>
    /// <param name="sender">The sender of the message. If left empty the addon's assembly name will be used.</param>
    /// <returns>The <see cref="Message"/> that was sent.</returns>
    public Message Send(Player to, string subject, string content, string sender = "");
}