using CitizenFX.Core;
using FivePD.Common.Attributes;

namespace FivePD.Gamemode.Common.DTO;

#pragma warning disable

[NoUsage]
internal sealed record FPedTasksDTO
{
    public record struct SetPedTaskRequest
    {
        public int NetworkId { get; set; }
    
        public string Task { get; set; }
    
        public string Args { get; set; }
    }

    public record struct StandStill
    {
        public required int Duration { get; init; }
    }
    
    public record struct TurnToEntity
    {
        public required int EntityNetworkId { get; init; }
    
        public required int Duration { get; init; }
    }
    
    public record struct TurnToPosition
    {
        public required Vector3 Position { get; init; }
    
        public required int Duration { get; init; }
    }

    public sealed record Events
    {
        public const string StandStill = "StandStill";

        public const string TurnToEntity = "TurnToEntity";

        public const string TurnToPosition = "TurnToPosition";
    }
}