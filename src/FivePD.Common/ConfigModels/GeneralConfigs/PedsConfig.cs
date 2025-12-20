using System.Collections.Generic;

namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record PedsConfig
{
    public int ChanceToReuseData { get; init; }

    public PedLicensesConfig Licenses { get; init; } = new();

    public WarrantsConfig Warrants { get; init; } = new();

    public AlcoholConfig Alcohol { get; init; } = new();

    public List<DrugConfig> Drugs { get; init; } = new();

    public BehaviorConfig Behavior { get; init; } = new();
}

public sealed record BehaviorConfig
{
    public int MinIntervalInMs { get; init; }

    public int MaxIntervalInMs { get; init; }

    public BehaviorChancesConfig Chances { get; init; } = new();
}

public sealed record BehaviorChancesConfig
{
    public PedBehaviorOnFootConfig OnFoot { get; init; } = new();

    public PedBehaviorInVehicleConfig InVehicle { get; init; } = new();
}

public sealed record PedBehaviorOnFootConfig
{
    public int Flee { get; init; }

    public int ShootAtTargets { get; init; }

    public int FindVehicleAndFlee { get; init; }

    public int StandStill { get; init; }

    public int DoNothing { get; init; }
}

public sealed record PedBehaviorInVehicleConfig
{
    public int DriveVehicleFlee { get; init; }

    public int LeaveVehicle { get; init; }

    public int DriveVehiclePullOver { get; init; }

    public int DoNothing { get; init; }
}