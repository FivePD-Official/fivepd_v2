namespace FivePD.Server.Models.Requests;

#pragma warning disable

internal record struct UpdatePlayerDetailsRequest
{
    public int Id { get; set; }
    
    public string Callsign { get; set; }
}