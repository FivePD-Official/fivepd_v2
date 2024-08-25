using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models.Requests;
using FivePD.Server.Attributes;
using FivePD.Server.Database;
using FivePD.Server.Extensions;
using FivePD.Server.Interfaces;
using FivePD.Server.Models.LoadoutModels;
using FivePD.Server.Models.Requests;
using FivePD.Server.QueryHelpers;
using Microsoft.EntityFrameworkCore;

namespace FivePD.Server.Services;

#pragma warning disable

public class WeaponService : IWeaponService
{
    private readonly DatabaseContext _databaseContext;

    public WeaponService(DatabaseContext databaseContext)
    {
        this._databaseContext = databaseContext;
    }

    [UIEvent(Events.UI.Weapon.Get)]
    private void GetById(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var weapon = this._databaseContext.Weapons
            .Include(x => x.WeaponWeaponComponents)
            .Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                Model = x.Model,
                WeaponComponentIds = x.WeaponWeaponComponents.Select(wwc => wwc.WeaponComponentId)
            })
            .FirstOrDefault(x => x.Id == request.Id);

        if (weapon is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        networkCallbackDelegate.InvokeWithJson(weapon);
    }

    [UIEvent(Events.UI.Weapon.GetAllPageable)]
    private void GetAllPageable(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<WeaponsRequest>();
        var filters = new Filters<Weapon>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Name),
            weapon => weapon.Name.ToLower().Contains(request.Filters.Name.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Model),
            weapon => weapon.Model.ToLower().Contains(request.Filters.Model.ToLower()));
        var page = this._databaseContext.Weapons
            .Include(x => x.WeaponWeaponComponents)
            .AsNoTracking()
            .Paginate(request.Page, filters);

        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.Weapon.GetAllSimple)]
    private void GetAllSimple(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        networkCallbackDelegate.InvokeWithJson(this._databaseContext.Weapons.Select(x => new
        {
            Id = x.Id,
            Name = x.Name,
        }));
    }

    [UIEvent(Events.UI.Weapon.Create)]
    private void Create(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<AddWeaponRequest>();

        var weapon = new Weapon() { Name = request.Name, Model = request.Model };

        foreach (var weaponComponentHashKey in request.WeaponComponentHashes)
        {
            var component = this._databaseContext.WeaponComponents.FirstOrDefault(x => x.HashKey == weaponComponentHashKey);

            if (component is not null)
            {
                weapon.AddComponent(component);
            }
        }

        this._databaseContext.Weapons.Add(weapon);
        this._databaseContext.SaveChanges();

        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.CREATE", "UI.LABEL.LOADOUT_CONFIG.WEAPON.SUCCESS.CREATE");
    }

    [UIEvent(Events.UI.Weapon.Edit)]
    private void Edit(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<EditWeaponRequest>();
        var weapon = this._databaseContext.Weapons.Include(x => x.WeaponWeaponComponents).FirstOrDefault(x => x.Id == request.Id);
        
        if (weapon is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        weapon.Name = request.Name;
        weapon.Model = request.Model;

        var existingWeaponComponents = weapon.WeaponWeaponComponents.Select(x => x.WeaponComponent.HashKey).ToList();
        var weaponComponentsToAdd = request.WeaponComponentHashes.Except(existingWeaponComponents).ToList();
        var weaponComponentsToRemove = weapon.WeaponWeaponComponents.Where(x => !request.WeaponComponentHashes.Contains(x.WeaponComponent.HashKey)).ToList();

        foreach (var weaponComponentHash in weaponComponentsToAdd)
        {
            var weaponComponent = this._databaseContext.WeaponComponents.FirstOrDefault(x => x.HashKey == weaponComponentHash);

            if (weaponComponent is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            weapon.AddComponent(weaponComponent);
        }
        
        foreach (var weaponWeaponComponent in weaponComponentsToRemove)
        {
            var weaponComponent = this._databaseContext.WeaponComponents.Find(weaponWeaponComponent.WeaponComponentId);

            if (weaponComponent is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            weapon.RemoveComponent(weaponWeaponComponent);
        }

        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.EDIT", "UI.LABEL.LOADOUT_CONFIG.WEAPON.SUCCESS.EDIT");
    }

    [UIEvent(Events.UI.Weapon.Delete)]
    private void Delete(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var weapon = this._databaseContext.Weapons.FirstOrDefault(x => x.Id == request.Id);
        
        if (weapon is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        this._databaseContext.Weapons.Remove(weapon);
        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.DELETE", "UI.LABEL.LOADOUT_CONFIG.WEAPON.SUCCESS.DELETE");
    }
}