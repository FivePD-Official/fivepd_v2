using FivePD.Common.Attributes;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

[Obfuscate]
internal sealed record DebugInfoDTO
{
    public bool CanAccessFivePD { get; init; }
    public bool IsInDepartment { get; init; }
    public bool IsInDivision { get; init; }
    public bool IsOnDuty { get; init; }
    public string PreferredLanguage { get; init; }
}