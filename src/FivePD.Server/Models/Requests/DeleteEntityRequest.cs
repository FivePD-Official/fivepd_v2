namespace FivePD.Server.Models.Requests;

#pragma warning disable

internal record struct DeleteEntityRequest
{
    public string SetId { get; set; }

    public string Id { get; set; }
}