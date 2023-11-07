using System;
using FivePD.Common.Enums;
using FivePD.Server.Attributes;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

[Obfuscate]
internal record struct SimplePedHistoryDTO
{
    public int Id { get; init; }

    public string Firstname { get; init; }

    public string Lastname { get; init; }

    public DateTime Birthdate { get; init; }

    public FGender Gender { get; init; }
}