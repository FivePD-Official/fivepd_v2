using System.Dynamic;

#pragma warning disable

namespace FivePD.Gamemode.Server.Models.Requests;

internal record struct AddEntityRequest
{
    public string SetId { get; set; }

    public int EntityId { get; set; }

    public ExpandoObject Entity { get; set; }
}