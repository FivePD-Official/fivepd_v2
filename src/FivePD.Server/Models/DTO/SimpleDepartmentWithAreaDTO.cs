using System.Collections.Generic;
using FivePD.Server.Attributes;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

[Obfuscate]
internal record struct SimpleDepartmentWithAreaDTO
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Callsign { get; set; }

    public List<DepartmentAreaCoordinate> Coordinates { get; set; }
}