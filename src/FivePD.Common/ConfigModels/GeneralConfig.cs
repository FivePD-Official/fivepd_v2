using System.Collections.Generic;
using FivePD.Common.ConfigModels.GeneralConfigs;

namespace FivePD.Common.ConfigModels;

#pragma warning disable

internal interface IInternalGeneralConfig
{
    public DatabaseConfig Database { get; set; }
}

public sealed record GeneralConfig : IInternalGeneralConfig
{
    private DatabaseConfig _database = new();

    public DatabaseConfig Database
    {
        get => this._database;
        init => this._database = value;
    }

    DatabaseConfig IInternalGeneralConfig.Database
    {
        get => this._database;
        set => this._database = value;
    }

    public UnitConfig Unit { get; init; } = new();

    public CalloutConfig Callout { get; init; } = new();

    public List<DispatchActionItemConfig> DispatchActions { get; init; } = new();

    public FeatureFlagsConfig Features { get; init; } = new();

    public ProgressionSystemConfig ProgressionSystem { get; init; } = new();

    public PedsConfig Peds { get; init; } = new();

    public VehiclesConfig Vehicles { get; init; } = new();
}