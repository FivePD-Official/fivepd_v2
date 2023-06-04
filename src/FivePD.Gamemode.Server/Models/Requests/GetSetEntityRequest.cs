#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests;

internal record struct GetSetEntityRequest
{
    public string SetId { get; set; }

    public int Id { get; set; }
}