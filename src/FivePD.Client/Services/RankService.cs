using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;

namespace FivePD.Client.Services;

#pragma warning disable

internal class RankService : IRankService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    public RankService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Rank.Get);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Rank.GetAllGlobal);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Rank.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Rank.Create);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Rank.CreateGlobal);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Rank.Edit);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Rank.EditGlobal);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.Rank.Delete);
    }
}