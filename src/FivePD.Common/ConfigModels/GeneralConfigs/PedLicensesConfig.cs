namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record PedLicensesConfig
{
    public PedLicenseConfig Driving { get; init; } = new();

    public PedLicenseConfig Fishing { get; init; } = new();

    public PedLicenseConfig Hunting { get; init; } = new();

    public PedLicenseConfig Ccw { get; init; } = new();

    public sealed record PedLicenseConfig
    {
        public int ChanceToHave { get; init; }

        public int ChanceForExpired { get; init; }
    }
}