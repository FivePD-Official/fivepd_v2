namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record LicensesConfig
{
    public LicenseConfig Driving { get; init; } = new();

    public LicenseConfig Fishing { get; init; } = new();

    public LicenseConfig Hunting { get; init; } = new();

    public LicenseConfig Ccw { get; init; } = new();

    public sealed record LicenseConfig
    {
        public int ChanceToHave { get; init; }

        public int ChanceForExpired { get; init; }
    }
}