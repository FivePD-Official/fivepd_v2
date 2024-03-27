namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record MenusConfig
{
    public MenuConfig VehicleSelector { get; init; } = new();

    public MenuConfig LoadoutSelector { get; init; } = new();

    public MenuConfig Dispatch { get; init; } = new();

    public MenuConfig Assistance { get; init; } = new();

    public MenuConfig Interaction { get; init; } = new();

    public sealed record MenuConfig
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
    }
}