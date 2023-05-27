namespace FivePD.Common.Requests;

#pragma warning disable

public record struct SetPedTaskRequest
{
    public int NetworkId { get; set; }
    
    public string Task { get; set; }
    
    public string Args { get; set; }
}