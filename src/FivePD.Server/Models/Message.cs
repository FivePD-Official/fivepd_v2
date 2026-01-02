using System;

namespace FivePD.Server.Models;

/// <summary>
/// Represents a message sent by a player or an addon.
/// </summary>
public class Message
{
    /// <summary>
    /// Gets or sets the identifier of the message.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the sender of the message.
    /// Either a player's name or an addons name.
    /// </summary>
    public string Sender { get; set; }

    /// <summary>
    /// Gets or sets the subject of the message.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the date and time the message was sent.
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the message has been read.
    /// </summary>
    public bool HasRead { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user the message was sent to.
    /// </summary>
    public User User { get; set; }
}