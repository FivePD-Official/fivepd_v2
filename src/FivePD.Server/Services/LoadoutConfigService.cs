using System.Collections.Generic;
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
using Serilog;

namespace FivePD.Server.Services;

#pragma warning disable

public class LoadoutConfigService : ILoadoutConfigService
{
    private readonly ILogger _logger;
    private readonly DatabaseContext _databaseContext;
    private readonly IMenuService _menuService;

    public LoadoutConfigService(ILogger logger, DatabaseContext databaseContext, IMenuService menuService)
    {
        this._logger = logger;
        this._databaseContext = databaseContext;
        this._menuService = menuService;
    }

    public List<LoadoutConfig> GetAll()
    {
        return this._databaseContext.LoadoutConfigs.ToList();
    }

    [UIEvent(Events.UI.LoadoutConfig.Get)]
    private void GetUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var loadoutConfig = this._databaseContext.LoadoutConfigs
            .Include(x => x.LoadoutWeapons)
            .Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                WeaponIds = x.LoadoutWeapons.Select(wid => wid.WeaponId),
            })
            .FirstOrDefault(x => x.Id == request.Id);

        if (loadoutConfig is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        networkCallbackDelegate.InvokeWithJson(loadoutConfig);
    }

    [UIEvent(Events.UI.LoadoutConfig.GetAllPageable)]
    private void GetAllPageableUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<LoadoutConfigsRequest>();
        var filters = new Filters<LoadoutConfig>();
        filters.Add(
            !string.IsNullOrEmpty(request.Filters.Name),
            loadoutConfig => loadoutConfig.Name.ToLower().Contains(request.Filters.Name.ToLower()));
        var page = this._databaseContext.LoadoutConfigs
            .AsNoTracking()
            .Paginate(request.Page, filters);

        networkCallbackDelegate.InvokeWithJson(page);
    }

    [UIEvent(Events.UI.LoadoutConfig.Create)]
    private void CreateLoadoutConfigUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<AddLoadoutConfigRequest>();

        var loadoutConfig = new LoadoutConfig { Name = request.Name };

        foreach (var weaponId in request.WeaponIds)
        {
            var weapon = this._databaseContext.Weapons.FirstOrDefault(x => x.Id == weaponId);

            if (weapon is not null)
            {
                loadoutConfig.AddWeapon(weapon);
            }
        }

        this._databaseContext.LoadoutConfigs.Add(loadoutConfig);
        this._databaseContext.SaveChanges();

        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.CREATE", "UI.LABEL.LOADOUT_CONFIG.SUCCESS.CREATE");
    }

    [UIEvent(Events.UI.LoadoutConfig.Edit)]
    private void EditLoadoutConfigUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<EditLoadoutConfigRequest>();
        var loadout = this._databaseContext.LoadoutConfigs.Include(x => x.LoadoutWeapons).FirstOrDefault(x => x.Id == request.Id);
        
        if (loadout is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        loadout.Name = request.Name;
        
        var existingWeapons = loadout.LoadoutWeapons.Select(x => x.WeaponId).ToList();
        var weaponsToAdd = request.WeaponIds.Except(existingWeapons).ToList();
        var weaponsToRemove = loadout.LoadoutWeapons.Where(x => !request.WeaponIds.Contains(x.WeaponId)).ToList();

        foreach (var weaponId in weaponsToAdd)
        {
            var weapon = this._databaseContext.Weapons.Find(weaponId);

            if (weapon is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            loadout.AddWeapon(weapon);
        }
        
        foreach (var loadoutWeapon in weaponsToRemove)
        {
            var weapon = this._databaseContext.Weapons.Find(loadoutWeapon.WeaponId);

            if (weapon is null)
            {
                networkCallbackDelegate.InvokeWithUnexpectedError();
                return;
            }

            loadout.RemoveWeapon(loadoutWeapon);
        }

        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.EDIT", "UI.LABEL.LOADOUT_CONFIG.SUCCESS.EDIT");
    }

    [UIEvent(Events.UI.LoadoutConfig.Delete)]
    private void DeleteLoadoutConfigUI(ExpandoObject expandoObject, NetworkCallbackDelegate networkCallbackDelegate)
    {
        var request = expandoObject.ConvertTo<IdRequest>();
        var loadout = this._databaseContext.LoadoutConfigs.FirstOrDefault(x => x.Id == request.Id);
        
        if (loadout is null)
        {
            networkCallbackDelegate.InvokeWithUnexpectedError();
            return;
        }

        this._databaseContext.LoadoutConfigs.Remove(loadout);
        this._databaseContext.SaveChanges();
        networkCallbackDelegate.InvokeWithResponse(true, "UI.LABEL.SUCCESS.DELETE", "UI.LABEL.LOADOUT_CONFIG.SUCCESS.DELETE");
    }
}