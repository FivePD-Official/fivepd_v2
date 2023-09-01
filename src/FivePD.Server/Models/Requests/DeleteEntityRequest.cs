#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Server.Models.Requests;

internal record struct DeleteEntityRequest
{
    public string SetId { get; set; }

    public string Id { get; set; }
}