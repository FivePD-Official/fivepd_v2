using System.Collections.Generic;
using CitizenFX.Core;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface INetworkEventService
{
    // TODO: add some RegisterEvent method which would handle this somehow better
    public void TriggerEvent(Player player, string guid, params object[] args);

    public void TriggerEventSerialized(Player player, string guid, object obj);

    public void TriggerEventWithResponse(Player player, string guid, bool status, string title, string description, Dictionary<string, string> @params = null);

    public void TriggerEventWithUnexpectedError(Player player, string guid);
}