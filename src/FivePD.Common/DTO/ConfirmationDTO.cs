using FivePD.Common.Models;

namespace FivePD.Client.DTO;

#pragma warning disable

internal sealed record ConfirmationDTO
{
    public int TimeoutInMs { get; init; }

    public ConfirmationProperties Properties { get; init; }
} 