using System.Collections.Generic;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct AddEntityRequest
{
    public string SetId { get; set; }

    public string EntityId { get; set; }

    public Dictionary<string, object> Entity { get; set; }
}