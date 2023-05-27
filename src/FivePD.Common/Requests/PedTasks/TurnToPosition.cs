using CitizenFX.Core;

namespace FivePD.Common.Requests.PedTasks;

#pragma warning disable

public record struct TurnToPosition
{
    public Vector3 Position { get; set; }
    
    public int Duration { get; set; }
}