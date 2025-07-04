namespace FivePD.Client.Interfaces;

#pragma warning disable

internal interface INetworkEventService
{
    void TriggerServerEvent(string eventName, params object[] args);
}
