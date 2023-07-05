using FivePD.Common.Models;

namespace FivePD.Common.DTO;

#pragma warning disable

internal sealed record MarkerDTO
{
    public readonly record struct Add(string Guid, Marker Marker);
}