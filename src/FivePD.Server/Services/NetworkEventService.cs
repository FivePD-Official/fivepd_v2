using System;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Server.Interfaces;

namespace FivePD.Server.Services;

#pragma warning disable

public class NetworkEventService : INetworkEventService
{
    public void TriggerEvent(Player player, string guid, params object[] args)
    {
        player.TriggerEvent($"{Events.NetworkEvent.ClientToServer}:{guid}", args);
    }
}