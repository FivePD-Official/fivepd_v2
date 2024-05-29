using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IWeaponComponentService" />
internal class WeaponComponentService : IWeaponComponentService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponComponentService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public WeaponComponentService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.WeaponComponent.Get);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.WeaponComponent.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.WeaponComponent.GetAllSimple);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.WeaponComponent.Create);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.WeaponComponent.Delete);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.WeaponComponent.Edit);
    }
}