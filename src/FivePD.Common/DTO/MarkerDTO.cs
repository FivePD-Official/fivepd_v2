using FivePD.Common.Models;

namespace FivePD.Gamemode.Client.DTO;

#pragma warning disable

public sealed record MarkerDTO
{
    public readonly record struct Add(string Guid, Marker Marker);
}