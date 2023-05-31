using FivePD.Common.Attributes;
using FivePD.Gamemode.Common.Utilities;

namespace FivePD.Gamemode.Client.Requests;

#pragma warning disable

[NoUsage]
internal sealed record FPedPropertiesDTO
{
    public record struct CreateBlipRequest
    {
        public FBlipSprite Sprite { get; set; }

        public FBlipColor Color { get; set; }

        public string Text { get; set; }
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