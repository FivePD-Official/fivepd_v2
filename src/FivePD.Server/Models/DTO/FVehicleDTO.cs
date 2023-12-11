using FivePD.Server.Attributes;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

[Obfuscate]
internal record struct FVehicleDTO
{
    public int Id { get; init; }

    public int NetworkId { get; init; }

    public string LicensePlate { get; init; }

    public string Color { get; init; }

    public string Brand { get; init; }

    public PedHistory Owner { get; init; }
}