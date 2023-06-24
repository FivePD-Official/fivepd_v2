using System.Collections.Generic;
using CitizenFX.Core;

namespace FivePD.Gamemode.Client.DTO;

#pragma warning disable

public sealed record CalloutDTO
{
    public sealed record CalloutToClientDTO
    {
        public string Title { get; init; }

        public string Description { get; init; }
        
        public Vector3 Location { get; init; }

        public IEnumerable<string> Units { get; init; }
    }
    
    public sealed record CalloutToUIDTO
    {
        public string Title { get; init; }

        public string Description { get; init; }
        
        public string Address { get; init; }

        public IEnumerable<string> Units { get; init; }
    }
    
    public sealed record CalloutTypeToUIDTO
    {
        public string TypeName { get; init; }

        public string AssemblyName { get; init; }
    }
}