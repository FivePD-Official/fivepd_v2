using FivePD.Gamemode.Common.Utilities;

namespace FivePD.Common.Requests;

#pragma warning disable

public record struct CreateBlipRequest
{
    public int NetworkId { get; set; }

    public FBlipSprite Sprite { get; set; }

    public FBlipColor Color { get; set; }

    public string Text { get; set; }
}