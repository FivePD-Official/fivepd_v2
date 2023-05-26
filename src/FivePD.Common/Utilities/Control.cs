using System;
using CitizenFX.Core;
using FivePD.Common.Attributes;

#pragma warning disable

namespace FivePD.Gamemode.Common.Utilities;

[NoUsage]
public class Control
{
    public string Id { get; init; }

    public string Description { get; init; }

    public string Key { get; init; }

    public string KeyMapper { get; init; }

    public Action<Player> OnPress { get; init; } = null;

    public Action<Player> OnRelease { get; init; } = null;
    
    public int TimeToHoldInMs { get; init; } = 0;

    public ClientControlResponse ToClientResponse()
    {
        return new ClientControlResponse
        {
            Id = this.Id,
            Description = this.Description,
            Key = this.Key,
            KeyMapper = this.KeyMapper,
            TimeToHoldInMs = this.TimeToHoldInMs,
        };
    }

    public class ClientControlResponse
    {
        public int Hashcode { get; init; }

        public string Id { get; init; }

        public string Description { get; init; }

        public string Key { get; init; }

        public string KeyMapper { get; init; }
        
        public int TimeToHoldInMs { get; init; }
    }
}
