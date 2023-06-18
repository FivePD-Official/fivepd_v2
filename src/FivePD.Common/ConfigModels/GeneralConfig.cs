using System.Collections.Generic;
using FivePD.Gamemode.Common.ConfigModels.GeneralConfigs;

namespace FivePD.Gamemode.Common.ConfigModels;

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

    public List<StatusItemConfig> Status { get; init; } = new();

    public FeatureFlagsConfig FeatureFlags { get; init; } = new();
}