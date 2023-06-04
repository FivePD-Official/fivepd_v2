using FivePD.Gamemode.Common.Attributes;
using FivePD.Gamemode.Common.Enums;

namespace FivePD.Gamemode.Common.DTO;

#pragma warning disable

[NoUsage]
internal sealed record FPedPropertiesDTO
{
    public record struct CreateBlipRequest
    {
        public required FBlipSprite Sprite { get; init; }

        public required FBlipColor Color { get; init; }

        public required string Text { get; init; }
    }

    public sealed record Events
    {
        public const string IsDead = "IsDead";

        public const string IsOnFoot = "IsOnFoot";

        public const string IsInVehicle = "IsInVehicle";

        public const string BlockPermanentEvents = "BlockPermanentEvents";

        public const string AlwaysKeepTask = "SetKeepTask";

        public const string CreateBlip = "CreateBlip";

        public const string RemoveBlips = "RemoveBlips";
    }
}