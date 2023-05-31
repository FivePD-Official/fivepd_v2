using System.Collections.Generic;
using FivePD.Common.Attributes;

namespace FivePD.Gamemode.Client.Requests;

#pragma warning disable

[NoUsage]
internal sealed record NotificationDTO
{
    public record struct ShowTooltip
    {
        public required string Content { get; init; }

        public required bool WithBeep { get; init; }

        public required Dictionary<string, string> ContentParams { get; init; }
    }

    public record struct ShowNotification
    {
        public required string Content { get; init; }

        public required Dictionary<string, string> ContentParams { get; init; }
    }

    public record struct ShowNotificationWithTexture
    {
        public required string Texture { get; init; }

        public required string Sender { get; init; }

        public required string Subject { get; init; }

        public required string Content { get; init; }

        public required Dictionary<string, string> ContentParams { get; init; }
    }
}