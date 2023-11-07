using System;
using CitizenFX.Core;

#pragma warning disable

namespace FivePD.Common.DTO;

internal sealed record ControlsDTO
{
    public string Id { get; init; }

    public string Title { get; init; }

    public string Key { get; init; }

    public string KeyMapper { get; init; }

    public Action<Player> OnPress { get; init; } = null;

    public Action<Player> OnRelease { get; init; } = null;
    
    public int TimeToHoldInMs { get; init; } = 0;

    public ClientControlDTO ToClientDTO()
    {
        return new ClientControlDTO
        {
            Id = this.Id,
            Title = this.Title,
            Key = this.Key,
            KeyMapper = this.KeyMapper,
            TimeToHoldInMs = this.TimeToHoldInMs,
        };
    }

    public class ClientControlDTO
    {
        public string Id { get; init; }

        public string Title { get; init; }

        public string Key { get; init; }

        public string KeyMapper { get; init; }
        
        public int TimeToHoldInMs { get; init; }
    }
}
