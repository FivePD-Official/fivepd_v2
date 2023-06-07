using System.Collections.Generic;

namespace FivePD.Gamemode.Client.DTO;

#pragma warning disable

internal sealed record ConfirmationDTO
{
    public int Key { get; init; }

    public int TimeoutInMs { get; init; }

    public string Tooltip { get; init; }

    public string Notification { get; init; } // {{timeout}} param will be automatically replaced
    
    public Dictionary<string, string> NotificationParams { get; init; }
} 