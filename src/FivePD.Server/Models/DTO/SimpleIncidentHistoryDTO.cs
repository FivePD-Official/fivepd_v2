using System;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

internal record struct SimpleIncidentHistoryDTO
{
    public string Id { get; init; }
    
    public string Title { get; init; }
    
    public DateTime GeneratedAt { get; init; }
}