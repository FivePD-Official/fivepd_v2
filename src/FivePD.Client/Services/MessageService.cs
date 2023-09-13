using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Common;

namespace FivePD.Client.Services;

#pragma warning disable

internal class MessageService : IMessageService, IServiceWithEvents
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;
    
    public MessageService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    public void RegisterEvents()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Message.Get);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Message.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Message.Send);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Message.Read);
    }
}