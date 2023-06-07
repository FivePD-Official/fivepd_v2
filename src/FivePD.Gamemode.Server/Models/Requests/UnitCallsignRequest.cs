namespace FivePD.Gamemode.Server.Models.Requests;

#pragma warning disable

public record struct UnitCallsignRequest
{
    public string Callsign { get; init; }
}