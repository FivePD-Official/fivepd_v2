using FivePD.Common.Attributes;

namespace FivePD.Common.Requests;

#pragma warning disable

[NoUsage]
public record struct SetPedTaskRequest
{
    public int NetworkId { get; set; }
    
    public string Task { get; set; }
    
    public string Args { get; set; }
}