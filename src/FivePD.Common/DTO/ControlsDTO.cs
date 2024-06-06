#pragma warning disable

namespace FivePD.Common.DTO;

internal sealed record ControlsDTO
{
    public sealed record ClientControlDTO
    {
        public string Id { get; init; }

        public string Title { get; init; }

        public string Key { get; init; }

        public string KeyMapper { get; init; }

        public int TimeToHoldInMs { get; init; }
    }
}