using System.Collections.Generic;

namespace FivePD.Gamemode.Common.ConfigModels;

#pragma warning disable

public sealed record GeneralSettings
{
    public UnitSettings Unit { get; init; } = new();

    public CalloutSettings Callout { get; init; } = new();

    public StatusSettings Status { get; init; } = new();
    
    public sealed record UnitSettings
    {
        public string Join { get; init; } = "FREE";

        public bool JoinFreely()
        {
            return this.Join.ToUpper() == "FREE";
        }

        public bool JoinByRequest()
        {
            return this.Join.ToUpper() == "REQUEST";
        }
    }

    public sealed record CalloutSettings
    {
        public bool AutomaticGeneration { get; init; }

        public int MinIntervalInMs { get; init; }

        public int MaxIntervalInMs { get; init; }
    }

    public sealed record StatusSettings
    {
        public List<StatusObject> Codes { get; init; } = new();

        public List<StatusObject> Signals { get; init; } = new();
        
        public sealed record StatusObject
        {
            public string Title { get; init; }
            
            public string Description { get; init; }
            
            public StatusOptions Options { get; init; }
        }

        public sealed record StatusOptions
        {
            public bool TogglesAvailability { get; init; }   

            public bool CompletesCurrentIncident { get; init; }   

            public bool TriggersPanicMode { get; init; }   
        }
    }
}