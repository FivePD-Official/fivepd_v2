using FivePD.Server.Attributes;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

[Obfuscate]
internal record struct SimpleDepartmentDTO
{
    public int Id { get; set; }

    public string Name { get; set; }
}