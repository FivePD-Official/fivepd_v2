using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common.Enums;

namespace FivePD.Common.DTO;

#pragma warning disable

public sealed record IncidentDTOs
{
    public record struct IncidentToClientDTO
    {
        public string Id { get; init; }

        public FIncidentType Type { get; init; }

        public string Title { get; init; }

        public string Description { get; init; }
        
        public Vector3 Location { get; init; }

        public IEnumerable<string> Units { get; init; }
    }
    
    public record struct IncidentToUIDTO
    {
        public string Id { get; init; }
        
        public FIncidentType Type { get; init; }

        public string Title { get; init; }

        public string Description { get; init; }
        
        public string Address { get; init; }

        public IEnumerable<string> Units { get; init; }
    }
    
    public record struct CalloutTypeToUIDTO
    {
        public string TypeName { get; init; }

        public string AssemblyName { get; init; }
    }
}