using System.Collections.Generic;

namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record MenusConfig
{
    public MenuConfig VehicleSelector { get; init; } = new();

    public MenuConfig LoadoutSelector { get; init; } = new();

    public DispatchMenuConfig Dispatch { get; init; } = new();

    public MenuConfig Assistance { get; init; } = new();

    public MenuConfig Interaction { get; init; } = new();

    public SceneManagerMenuConfig SceneManager { get; init; } = new();

    public QuestioningMenuConfig Questioning { get; init; } = new();

    public record MenuConfig
    {
        public bool Enabled { get; init; } = true;

        public string Access { get; init; } = "EVERYWHERE";

        public bool AccessEverywhere()
        {
            return this.Access.ToUpper() == "EVERYWHERE";
        }

        public bool AccessOnlyVehicle()
        {
            return this.Access.ToUpper() == "ONLY_VEHICLE";
        }

        public bool AccessOnlySpecificLocations()
        {
            return this.Access.ToUpper() == "ONLY_SPECIFIC_LOCATIONS";
        }

        public bool AccessOnlyVehicleAndSpecificLocations()
        {
            return this.Access.ToUpper() == "ONLY_VEHICLE_AND_SPECIFIC_LOCATIONS";
        }
        
        public bool MustBeOnDuty { get; init; } = false;
        
        public bool MustBeInDepartment { get; init; } = false;
    }

    public sealed record DispatchMenuConfig : MenuConfig
    {
        public List<DispatchMenuItemConfig> Items { get; init; } = new();
    }

    public sealed record SceneManagerMenuConfig : MenuConfig
    {
        public SpeedZoneConfig SpeedZone { get; init; } = new();

        public ObjectPlacementConfig ObjectPlacement { get; init; } = new();

        public sealed record SpeedZoneConfig
        {
            public bool Enabled { get; init; }

            public List<int> Speed { get; init; } = new();

            public List<int> Radius { get; init; } = new();
        }
        
        public sealed record ObjectPlacementConfig
        {
            public bool Enabled { get; init; }
            
            public List<ObjectConfig> Objects { get; init; } = new();

            public sealed record ObjectConfig
            {
                public string Name { get; init; }

                public string Model { get; init; }
            }
        }
    }
    
    public sealed record QuestioningMenuConfig : MenuConfig;
}