using CitizenFX.Core;

namespace FivePD.Gamemode.Common.DTO;

#pragma warning disable

internal sealed record FPedTasksDTO
{
    public record struct SetPedTaskRequest
    {
        public int NetworkId { get; set; }

        public string Task { get; set; }

        public string Args { get; set; }
    }

    public record struct StandStill
    {
        public required int Duration { get; init; }
    }

    public record struct TurnToEntity
    {
        public required int EntityNetworkId { get; init; }

        public required int Duration { get; init; }
    }

    public record struct TurnToPosition
    {
        public required Vector3 Position { get; init; }

        public required int Duration { get; init; }
    }

    public record struct DriveTo
    {
        public required int VehicleNetworkId { get; init; }

        public required Vector3 Target { get; init; }

        public required float Radius { get; init; }

        public required float Speed { get; init; }

        public required int DrivingStyle { get; init; }
    }
    
    public record struct CruiseWithVehicle
    {
        public required int VehicleNetworkId { get; init; }

        public required float Speed { get; init; }

        public required int DrivingStyle { get; init; }
    }

    public sealed record Events
    {
        public const string StandStill = "StandStill";

        public const string TurnToEntity = "TurnToEntity";

        public const string TurnToPosition = "TurnToPosition";

        public const string DriveTo = "DriveTo";
        
        public const string CruiseWithVehicle = "CruiseWithVehicle";
    }
}