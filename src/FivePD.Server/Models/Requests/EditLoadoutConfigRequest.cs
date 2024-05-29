using System.Collections.Generic;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct EditLoadoutConfigRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<int> WeaponIds { get; set; }
}