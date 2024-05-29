using System.Collections.Generic;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct EditWeaponRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Model { get; set; }
    public List<int> WeaponComponentIds { get; set; }
}