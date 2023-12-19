using System.Collections.Generic;

#pragma warning disable

namespace FivePD.Server.Models.Requests;

internal record struct CreateDepartmentRequest()
{
    public string Name { get; set; }

    public string Callsign { get; set; } = string.Empty;

    public List<CallsignFormatItem> CallsignFormat { get; set; }

    public int MaxUsers { get; set; }
}