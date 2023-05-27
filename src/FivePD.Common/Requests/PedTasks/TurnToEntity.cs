namespace FivePD.Common.Requests.PedTasks;

#pragma warning disable

public record struct TurnToEntity
{
    public int EntityNetworkId { get; set; }
    
    public int Duration { get; set; }
}