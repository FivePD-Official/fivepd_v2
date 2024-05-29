using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;

namespace FivePD.Client.Services;

/// <inheritdoc cref="FivePD.Client.Interfaces.IWeaponService" />
internal class WeaponService : IWeaponService, IOnInit
{
    private readonly EventHandlerDictionary _eventHandlerDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponService"/> class.
    /// </summary>
    /// <param name="eventHandlerDictionary">The <see cref="EventHandlerDictionary"/> to use.</param>
    public WeaponService(EventHandlerDictionary eventHandlerDictionary)
    {
        this._eventHandlerDictionary = eventHandlerDictionary;
    }

    /// <inheritdoc/>
    public void OnInit()
    {
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Weapon.Get);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Weapon.GetAllPageable);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Weapon.GetAllSimple);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Weapon.Create);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Weapon.Delete);
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Weapon.Edit);
    }
}