using CitizenFX.Core;

namespace FivePD.Server.Interfaces;

#pragma warning disable

public interface INetworkEventService
{
    // TODO: add some RegisterEvent method which would handle this somehow better
    public void TriggerEvent(Player player, string guid, params object[] args);
}