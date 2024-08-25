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
        this._eventHandlerDictionary.AddUIToServerEvent(Events.UI.Weapon.Delete);
        this._eventHandlerDictionary.AddUIEvent(
            Events.UI.Weapon.Create,
            new Action<ExpandoObject, CallbackDelegate>(this.CreateWeapon));
        this._eventHandlerDictionary.AddUIEvent(
            Events.UI.Weapon.Edit,
            new Action<ExpandoObject, CallbackDelegate>(this.EditWeapon));
    }

    private void CreateWeapon(ExpandoObject request, CallbackDelegate callbackDelegate)
    {
        var body = JsonConvert.DeserializeObject<AddWeaponRequest>(JsonConvert.SerializeObject(request));

        var isValid = this.CheckWeaponAndComponentValidity(body.Model, body.WeaponComponentHashes);

        if (!string.IsNullOrEmpty(isValid))
        {
            callbackDelegate.Invoke(isValid);
            return;
        }
        
        BaseScript.TriggerServerEvent(Events.UI.Weapon.Create, body, new Action<string>(data => { callbackDelegate.Invoke(data); }));
    }
    
    private void EditWeapon(ExpandoObject request, CallbackDelegate callbackDelegate)
    {
        var body = JsonConvert.DeserializeObject<EditWeaponRequest>(JsonConvert.SerializeObject(request));

        var isValid = this.CheckWeaponAndComponentValidity(body.Model, body.WeaponComponentHashes);

        if (!string.IsNullOrEmpty(isValid))
        {
            callbackDelegate.Invoke(isValid);
            return;
        }
        
        BaseScript.TriggerServerEvent(Events.UI.Weapon.Edit, body, new Action<string>(data => { callbackDelegate.Invoke(data); }));
    }

    public string CheckWeaponAndComponentValidity(string weaponModelName, List<string> componentHashes)
    {
        var modelNameAsHash = (uint)Cfx.API.GetHashKey(weaponModelName);
        var isValid = Cfx.API.IsWeaponValid(modelNameAsHash);
        
        if (!isValid)
        {
            var response = new
            {
                Status = false,
                Title = "UI.ERROR.WEAPON.HASH_NOT_VALID.TITLE",
                Description = "UI.ERROR.WEAPON.HASH_NOT_VALID.DESCRIPTION",
                Params = new Dictionary<string, string>(){{"model", weaponModelName}}
            };
            
            return JsonConvert.SerializeObject(response);
        }

        foreach (var componentHash in componentHashes)
        {
            var componentAsHash = (uint)Cfx.API.GetHashKey(componentHash);
            var isComponentValid = Cfx.API.DoesWeaponTakeWeaponComponent(modelNameAsHash, componentAsHash);

            if (!isComponentValid)
            {
                var response = new
                {
                    Status = false,
                    Title = "UI.ERROR.WEAPON_COMPONENT.HASH_NOT_VALID.TITLE",
                    Description = "UI.ERROR.WEAPON_COMPONENT.HASH_NOT_VALID.DESCRIPTION",
                    Params = new Dictionary<string, string>(){{"model", componentHash}}
                };
            
                return JsonConvert.SerializeObject(response);
            }
        }
        
        return string.Empty;
    }
}