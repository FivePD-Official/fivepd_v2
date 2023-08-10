namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record CalloutConfig
{
    public string DispatchType { get; init; } = "AUTO";
    
    public bool DispatchTypeAuto()
    {
        return this.DispatchType.ToUpper() == "AUTO";
    }

    public bool DispatchTypeManual()
    {
        return this.DispatchType.ToUpper() == "MANUAL";
    }

    public int MinIntervalInMs { get; init; }

    public int MaxIntervalInMs { get; init; }

    public int MaxActiveCallouts { get; init; }
    
    public DynamicDispatchConfig DynamicDispatch { get; init; } = new();

    public sealed record DynamicDispatchConfig
    {
        public bool Enabled { get; init; }

        public float Radius { get; init; }
    }
}