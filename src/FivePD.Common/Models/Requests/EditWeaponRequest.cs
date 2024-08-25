using System.Collections.Generic;

namespace FivePD.Common.Models.Requests;
#pragma warning disable

internal record struct EditWeaponRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Model { get; set; }
    public List<string> WeaponComponentHashes { get; set; }
}