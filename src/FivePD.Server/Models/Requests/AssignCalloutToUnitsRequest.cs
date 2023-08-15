using System.Collections.Generic;

namespace FivePD.Server.Models.Requests;

#pragma warning disable

internal record struct AssignCalloutToUnitsRequest
{
    public List<string> UnitCallsigns { get; set; }
    
    public string CalloutId { get; set; }
}