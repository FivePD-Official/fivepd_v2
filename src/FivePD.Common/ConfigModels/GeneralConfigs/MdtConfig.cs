namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record MdtConfig
{
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