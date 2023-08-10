#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct StringIdRequest
{
    public string Id { get; set; }
}