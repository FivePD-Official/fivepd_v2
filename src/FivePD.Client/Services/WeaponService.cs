using System;
using System.Collections.Generic;
using System.Dynamic;
using CitizenFX.Core;
using FivePD.Client.Extensions;
using FivePD.Client.Interfaces;
using FivePD.Client.Interfaces.Lifecycle;
using FivePD.Common;
using FivePD.Common.Models.Requests;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable

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
        this._eventHandlerDictionary.AddUIEvent(
            Events.UI.Weapon.IsWeaponValid,
            new Action<ExpandoObject, CallbackDelegate>(this.IsWeaponValid));
    }

    private void IsWeaponValid(ExpandoObject request, CallbackDelegate callbackDelegate)
    {
        var body = JsonConvert.DeserializeObject<AddWeaponRequest>(JsonConvert.SerializeObject(request));
        var modelNameAsHash = (uint)Cfx.API.GetHashKey(body.Model);
        var isValid = Cfx.API.IsWeaponValid(modelNameAsHash);
        
        if (!isValid)
        {
            var response = new
            {
                Status = false,
                Title = "UI.ERROR.WEAPON.HASH_NOT_VALID.TITLE",
                Description = "UI.ERROR.WEAPON.HASH_NOT_VALID.DESCRIPTION",
                Params = new Dictionary<string, string>(){{"model", body.Model}}
            };
            
            callbackDelegate.Invoke(JsonConvert.SerializeObject(response));
            return;
        }
        
        // Loop the components and call DoesWeaponTakeWeaponComponent
        // Passing in the wep hash from 48 and hashing the component
        // Capture a list of invalid components
        // Report to frontend
        foreach (var VARIABLE in body.)
        {
            
        }
        
        BaseScript.TriggerServerEvent(Events.UI.Weapon.Create, body, new Action<string>(data => { callbackDelegate.Invoke(data); }));
    }
}