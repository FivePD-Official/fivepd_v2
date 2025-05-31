using FivePD.Common.DTO;

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

    public bool AreaBasedDispatch { get; init; }

    public DynamicDispatchConfig DynamicDispatch { get; init; } = new();

    public AutoRemoveConfig AutoRemove { get; init; } = new();

    public sealed record DynamicDispatchConfig
    {
        public bool Enabled { get; init; }

        public float Radius { get; init; }
    }

    public sealed record AutoRemoveConfig
    {
        public bool Enabled { get; init; }

        public AutoRemoveTimeoutConfig TimeoutByDispatchType { get; init; }

        public sealed record AutoRemoveTimeoutConfig
        {
            public int Auto { get; init; }

            public int Manual { get; init; }
        }
    }
}