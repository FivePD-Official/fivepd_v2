using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Models;
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
    private readonly IGameNotificationService _gameNotificationService;
    private readonly ILoadoutConfigService _loadoutConfigService;
    private readonly ILocationService _locationService;
    private MenuListItem _changeLanguageListItem;

    public LoadoutSelectorMenu(IConfigService configService, IGameNotificationService gameNotificationService, ILoadoutConfigService loadoutConfigService, ILocationService locationService)
        : base(Controls.LoadoutSelectorMenu.Id, Controls.LoadoutSelectorMenu.Title, Controls.LoadoutSelectorMenu.Key)
    {
        this._configService = configService;
        this._gameNotificationService = gameNotificationService;
        this._loadoutConfigService = loadoutConfigService;
        this._locationService = locationService;
    }

    public MenuListItem LoadoutItem { get; private set; }

    public override bool CanOpen(FPlayer player)
    {
        var config = this._configService.GeneralConfig.Menu.LoadoutSelector;
        if (config.MustBeOnDuty && !player.IsOnDuty)
        {
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "ERROR.USER.NOT_ON_DUTY"
                });

            return false;
        }

        if (config.MustBeInDepartment && !player.IsInDepartment)
        {
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "ERROR.USER.NOT_IN_DEPARTMENT"
                });
            
            return false;
        }
        
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
            "LABEL.MENU.LOADOUT.GET.TITLE",
            "LABEL.MENU.LOADOUT.GET.DESCRIPTION",
            this._loadoutConfigService.GetAll().Select(loadout => new MenuListOption(loadout.Name)).ToList());
        this.LoadoutItem.OnSelect += async (player, index) =>
        {
            var playerPed = FPed.FromFPlayer(player);
            var loadout = this._loadoutConfigService.GetAll().ElementAt(index);
            foreach (var weapon in loadout.LoadoutWeapons)
            {
                Debug.WriteLine(weapon.Weapon.Model);
                var weaponHash = (FWeaponHash)Cfx.API.GetHashKey(weapon.Weapon.Model);
                playerPed.Weapons.Give(weaponHash, 100, true, true);

                foreach (var componentKey in weapon.Weapon.WeaponWeaponComponents)
                {
                    playerPed.Weapons.AddComponentToWeapon(weaponHash, (uint)Cfx.API.GetHashKey(componentKey.WeaponComponent.HashKey));
                }
            }
        };
        items.Add(this.LoadoutItem);

        return items;
    }
}