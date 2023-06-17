using System.Collections.Generic;

namespace FivePD.Gamemode.Common.ConfigModels;

#pragma warning disable

public sealed record GeneralConfig
{
    public DatabaseConfig Database { get; init; } = new();

    public UnitSettings Unit { get; init; } = new();

    public CalloutSettings Callout { get; init; } = new();

    public StatusSettings Status { get; init; } = new();
    
    public sealed record DatabaseConfig
    {
        /// <summary>
        /// Gets the address of the database.
        /// </summary>
        public string Address { get; init; }

        /// <summary>
        /// Gets the port of the database.
        /// </summary>
        public string Port { get; init; }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public string DatabaseName { get; init; }

        /// <summary>
        /// Gets the user used for connecting to the database.
        /// </summary>
        public string User { get; init; }

        /// <summary>
        /// Gets the password used for connecting to the database.
        /// </summary>
        public string Password { get; init; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"server={this.Address};port={this.Port};database={this.DatabaseName};user={this.User};password={this.Password};";
        }
    }
    
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