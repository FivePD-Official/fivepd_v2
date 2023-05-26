using System;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace FivePD.Gamemode.Server.Interfaces;

#pragma warning disable

public interface IEventService
{
    public void TriggerEventWithoutCallback(Player player, string eventName, params object[] args);

    public void TriggerEvent(Player player, string eventName, Action<string> callback, params object[] args);

    public Task<string> TriggerEventAsync(Player player, string eventName, params object[] args);
}