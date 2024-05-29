using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.ILoadoutConfigService" />
internal class LoadoutConfigService : ILoadoutConfigService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadoutConfigService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public LoadoutConfigService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LoadoutConfig.Get);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LoadoutConfig.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LoadoutConfig.Create);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LoadoutConfig.Delete);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.LoadoutConfig.Edit);
    }
}