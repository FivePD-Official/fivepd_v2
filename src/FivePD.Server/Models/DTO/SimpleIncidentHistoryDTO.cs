using System;

namespace FivePD.Server.Models.DTO;

#pragma warning disable

internal record struct SimpleIncidentHistoryDTO
{
    public int Id { get; init; }
    
    public string Title { get; init; }
    
    public DateTime GeneratedAt { get; init; }
}