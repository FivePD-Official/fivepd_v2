namespace FivePD.Client.DTO;

#pragma warning disable

internal sealed record RandomOffsetDTO
{
    public int EntityNetworkId { get; set; }

    public int OffsetMin { get; set; }

    public int OffsetMax { get; set; }
}