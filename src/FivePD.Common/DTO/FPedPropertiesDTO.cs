using FivePD.Common.Enums;

namespace FivePD.Common.DTO;

#pragma warning disable

internal sealed record FPedPropertiesDTO
{
    public record struct InvokeMethodRequest
    {
        public int NetworkId { get; set; }
    
        public string Method { get; set; }
    
        public string Args { get; set; }
    }

    public record struct CreateBlipRequest
    {
        public required FBlipSprite Sprite { get; init; }

        public required FBlipColor Color { get; init; }

        public required string Text { get; init; }
    }
    
    public record struct SetIntoVehicle
    {
        public required int VehicleNetworkId { get; init; }
    
        public required int VehicleSeat { get; init; }
    }

    public sealed record Events
    {
        public const string IsDead = "IsDead";

        public const string IsOnFoot = "IsOnFoot";

        public const string IsInVehicle = "IsInVehicle";

        public const string BlockPermanentEvents = "BlockPermanentEvents";

        public const string AlwaysKeepTask = "AlwaysKeepTask";

        public const string CreateBlip = "CreateBlip";

        public const string RemoveBlips = "RemoveBlips";

        public const string SetIntoVehicle = "SetIntoVehicle";

        public const string MarkAsNoLongerNeeded = "MarkAsNoLongerNeeded";
    }
}