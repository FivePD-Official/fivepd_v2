namespace FivePD.Server.Models.Requests;

#pragma warning disable

internal record struct AddWeaponComponentRequest
{
    public string Name { get; set; }
    public string HashKey { get; set; }
}