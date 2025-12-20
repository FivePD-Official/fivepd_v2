namespace FivePD.Common.ConfigModels.GeneralConfigs;

#pragma warning disable

public sealed record ProgressionSystemConfig
{
    public ExperiencePointsConfig ExperiencePoints { get; init; } = new();

    public sealed record ExperiencePointsConfig
    {
        public int CalloutCompleted { get; init; }

        public int TrafficStopCompleted { get; init; }

        public int VehicleImpounded { get; init; }
    }
}