using System.Collections.Generic;

namespace FivePD.Server.Models.Requests;

#pragma warning disable

internal record struct SendMessageRequest
{
    public List<int> To { get; init; }

    public string Subject { get; init; }
    
    public string Content { get; init; }
}