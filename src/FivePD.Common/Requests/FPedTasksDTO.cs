using CitizenFX.Core;
using FivePD.Common.Attributes;

namespace FivePD.Gamemode.Client.Requests;

#pragma warning disable

[NoUsage]
public sealed record FPedTasksDTO
{
    public record struct StandStill
    {
        public int Duration { get; set; }
    }
    
    public record struct TurnToEntity
    {
        public int EntityNetworkId { get; set; }
    
        public int Duration { get; set; }
    }
    
    public record struct TurnToPosition
    {
        public Vector3 Position { get; set; }
    
        public int Duration { get; set; }
    }

    public sealed class Events
    {
        public const string StandStill = "StandStill";

        public const string TurnToEntity = "TurnToEntity";

        public const string TurnToPosition = "TurnToPosition";
    }
}