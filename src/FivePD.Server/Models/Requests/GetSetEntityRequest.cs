#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct GetSetEntityRequest
{
    public string SetId { get; set; }

    public string Id { get; set; }
}