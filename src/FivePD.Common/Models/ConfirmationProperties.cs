using System.Collections.Generic;

namespace FivePD.Gamemode.Common.Models;

#pragma warning disable

public sealed record ConfirmationProperties
{
    public string Tooltip { get; init; }

    public required NotificationProperties Notification { get; init; }

    public sealed record NotificationProperties
    {
        public string Content { get; init; }

        public Dictionary<string, string> ContentParams { get; set; } = null;

        public string Texture { get; init; }

        public string Sender { get; init; }

        public string Subject { get; init; }
    }
}