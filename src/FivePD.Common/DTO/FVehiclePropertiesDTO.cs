using FivePD.Gamemode.Common.Enums;

namespace FivePD.Gamemode.Common.DTO;

#pragma warning disable

internal sealed record FVehiclePropertiesDTO
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

    public sealed record Events
    {
        public const string IsSirenActive = "IsSirenActive";

        public const string CreateBlip = "CreateBlip";

        public const string RemoveBlips = "RemoveBlips";
    }
}