namespace FivePD.Server.Models.Requests;

#pragma warning disable

internal record struct UnitCallsignRequest
{
    public string Callsign { get; init; }
}