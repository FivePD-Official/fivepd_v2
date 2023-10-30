using System.Collections.Generic;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct GetSetEntitiesRequest()
{
    public string SetId { get; set; }

    public Dictionary<string, object> Filter { get; set; } = new();
}