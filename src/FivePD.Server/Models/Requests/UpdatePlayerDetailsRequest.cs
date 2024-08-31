using System.Collections.Generic;

namespace FivePD.Server.Models.Requests;

#pragma warning disable

internal record struct UpdatePlayerDetailsRequest
{
    public int Id { get; set; }
    
    public List<string>? CallsignFormatValues { get; set; }

    public string? PreferredLanguage { get; set; }
}