#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct GetSetRequest
{
    public string SetId { get; set; }

    public int Page { get; set; }
}