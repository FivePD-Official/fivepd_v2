using CitizenFX.Core;

namespace FivePD.Gamemode.Client.Requests;

#pragma warning disable

public sealed class PedTasks
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
}