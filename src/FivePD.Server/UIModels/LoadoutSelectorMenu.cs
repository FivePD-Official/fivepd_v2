using System.Collections.Generic;
using System.Linq;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.UIModels.MenuModels;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S1450
#pragma warning disable S125
#pragma warning disable SA1515

namespace FivePD.Server.UIModels;

#pragma warning disable

public sealed class LoadoutSelectorMenu : Menu
{
    private readonly IConfigService _configService;
    private readonly IVehicleConfigService _vehicleConfigService;
    private readonly ILocationService _locationService;

    private MenuListItem _changeLanguageListItem;

    public LoadoutSelectorMenu(
        IConfigService configService,
        IVehicleConfigService vehicleConfigService,
        ILocationService locationService)
        : base(Controls.LoadoutSelectorMenu.Id, Controls.LoadoutSelectorMenu.Title, Controls.LoadoutSelectorMenu.Key)
    {
        this._configService = configService;
        this._vehicleConfigService = vehicleConfigService;
        this._locationService = locationService;
    }

    public MenuListItem LoadoutItem { get; private set; }

    public override bool CanOpen(FPlayer player)
    {
        var config = this._configService.GeneralConfig.Menu.LoadoutSelector;
        if (config.AccessEverywhere())
        {
            return true;
        }

        var playerIsInVehicle = player.FPed.Properties.IsInVehicle;
        if (config.AccessOnlyVehicle() && playerIsInVehicle)
        {
            return true;
        }

        var locations = this._locationService.GetAll(LocationType.LoadoutSelector);
        var isInRangeOfMarkedLocation = locations.Any(x => player.GetUnderlyingPlayer().Character.Position.DistanceToSquared(x.Position) <= 3f);
        if (config.AccessOnlySpecificLocations() && isInRangeOfMarkedLocation)
        {
            return true;
        }

        if (config.AccessOnlyVehicleAndSpecificLocations() && (playerIsInVehicle || isInRangeOfMarkedLocation))
        {
            return true;
        }

        return false;
    }

    protected override List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        this.LoadoutItem = new MenuListItem(
            "DUTY_MENU.ITEM.GET_LOADOUT.TITLE",
            "DUTY_MENU.ITEM.GET_LOADOUT.DESCRIPTION",
            this._configService.LoadoutsConfig.Select(x => new MenuListOption(x.Title)).ToList());
        this.LoadoutItem.OnSelect += async (player, index) =>
        {
            var playerPed = FPed.FromFPlayer(player);
            var loadout = this._configService.LoadoutsConfig.ElementAt(index);
            foreach (var weaponItem in loadout.Weapons)
            {
                var weaponHash = (FWeaponHash)Cfx.API.GetHashKey(weaponItem.Key);
                playerPed.Weapons.Give(weaponHash, weaponItem.Ammunition, false, true);

                foreach (var componentKey in weaponItem.ComponentKeys)
                {
                    playerPed.Weapons.AddComponentToWeapon(weaponHash, (uint)Cfx.API.GetHashKey(componentKey));
                }
            }
        };
        items.Add(this.LoadoutItem);

        return items;
    }
}