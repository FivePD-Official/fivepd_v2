using System.Dynamic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
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

public class WeaponComponentService : IWeaponComponentService
{
    private readonly DatabaseContext _databaseContext;

    public WeaponComponentService(DatabaseContext databaseContext)
    {
        this._databaseContext = databaseContext;
    }

    [UIEvent(Events.UI.WeaponComponent.Get)]
    private void GetById(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var weaponComponent = this._databaseContext.WeaponComponents.FirstOrDefault(x => x.Id == request.Id);

        if (weaponComponent is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        networkCallbackDelegate.InvokeWithJson(weaponComponent);
    }

    [UIEvent(Events.UI.WeaponComponent.GetAllPageable)]
    private void GetAllPageable(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<WeaponComponentsRequest>();
        var filters = new Filters<WeaponComponent>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Name),
            wc => wc.Name.ToLower().Contains(request.Filters.Name.ToLower()));
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.HashKey),
            wc => wc.HashKey.ToLower().Contains(request.Filters.HashKey.ToLower()));
        var page = this._databaseContext.WeaponComponents
            .AsNoTracking()
            .Paginate(request.Page, filters);

        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.WeaponComponent.GetAllSimple)]
    private void GetAllSimple(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        networkCallbackDelegate.InvokeWithJson(this._databaseContext.WeaponComponents.Select(x => new
        {
            Id = x.Id,
            Name = x.Name,
        }));
    }

    [UIEvent(Events.UI.WeaponComponent.Create)]
    private void Create(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<AddWeaponComponentRequest>();

        var weaponComponent = new WeaponComponent() { Name = request.Name, HashKey = request.HashKey };

        this._databaseContext.WeaponComponents.Add(weaponComponent);
        this._databaseContext.SaveChanges();

        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.CREATE", "UI.LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.SUCCESS.CREATE");
    }

    [UIEvent(Events.UI.WeaponComponent.Edit)]
    private void Edit(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<EditWeaponComponentRequest>();
        var weaponComponent = this._databaseContext.WeaponComponents.FirstOrDefault(x => x.Id == request.Id);
        
        if (weaponComponent is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }
        
        weaponComponent.Name = request.Name;
        weaponComponent.HashKey = request.HashKey;

        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.EDIT", "UI.LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.SUCCESS.EDIT");
    }

    [UIEvent(Events.UI.WeaponComponent.Delete)]
    private void Delete(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var weaponComponent = this._databaseContext.WeaponComponents.FirstOrDefault(x => x.Id == request.Id);
        
        if (weaponComponent is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        this._databaseContext.WeaponComponents.Remove(weaponComponent);
        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.DELETE", "UI.LABEL.LOADOUT_CONFIG.WEAPON_COMPONENT.SUCCESS.DELETE");
    }
}