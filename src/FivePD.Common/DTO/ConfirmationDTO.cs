using FivePD.Gamemode.Common.Models;

namespace FivePD.Gamemode.Client.DTO;

#pragma warning disable

internal sealed record ConfirmationDTO
{
    public int TimeoutInMs { get; init; }

    public ConfirmationProperties Properties { get; init; }
} 