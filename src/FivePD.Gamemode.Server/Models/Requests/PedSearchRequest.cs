#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests;

internal record struct PedSearchRequest
{
    public string Firstname { get; set; }

    public string Lastname { get; set; }
}