#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct EditWeaponComponentRequest
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string HashKey { get; set; }
}