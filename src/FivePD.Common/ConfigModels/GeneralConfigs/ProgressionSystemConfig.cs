namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record ProgressionSystemConfig
{
    public ExperiencePointsConfig ExperiencePoints { get; init; } = new();

    public sealed record ExperiencePointsConfig
    {
        public int Arrest { get; init; }

        public int CalloutCompleted { get; init; }

        public int Citation { get; init; }

        public int Fine { get; init; }

        public int Death { get; init; }

        public int VehicleImpounded { get; init; }
    }
}