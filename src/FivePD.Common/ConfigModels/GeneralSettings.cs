namespace FivePD.Gamemode.Common.ConfigModels;

#pragma warning disable

public sealed record GeneralSettings
{
    public UnitSettings Unit { get; init; } = new();

    public CalloutSettings Callout { get; init; } = new();
    
    public sealed record UnitSettings
    {
        public string Join { get; init; }

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
        public int MinIntervalInMs { get; init; }

        public int MaxIntervalInMs { get; init; }
    }
}