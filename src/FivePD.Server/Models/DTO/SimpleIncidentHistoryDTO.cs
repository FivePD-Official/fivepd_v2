using System;
using FivePD.Server.Attributes;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

[Obfuscate]
internal record struct SimpleIncidentHistoryDTO
{
    public string Id { get; init; }
    
    public string Title { get; init; }
    
    public DateTime GeneratedAt { get; init; }
}