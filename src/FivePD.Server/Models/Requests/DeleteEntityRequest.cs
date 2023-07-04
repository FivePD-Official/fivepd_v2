#pragma warning disable CS1591
#pragma warning disable SA1600

namespace FivePD.Server.Models.Requests;

internal record struct DeleteEntityRequest
{
    public string SetId { get; set; }

    public int Id { get; set; }
}