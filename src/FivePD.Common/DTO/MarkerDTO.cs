using FivePD.Gamemode.Common.Models;

namespace FivePD.Gamemode.Common.DTO;

#pragma warning disable

internal sealed record MarkerDTO
{
    public readonly record struct Add(string Guid, Marker Marker);
}