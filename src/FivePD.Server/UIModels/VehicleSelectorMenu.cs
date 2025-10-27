using System.Collections.Generic;
using System.Linq;
using FivePD.Common;
using FivePD.Common.DTO;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.UIModels.MenuModels;
using Newtonsoft.Json;
using Cfx = CitizenFX.Core.Native;

#pragma warning disable S1450
#pragma warning disable S125
#pragma warning disable SA1515

namespace FivePD.Server.UIModels;

#pragma warning disable

public sealed class VehicleSelectorMenu : Menu
{
    private readonly IConfigService _configService;
    private readonly IEntityService _entityService;
    private readonly IEventService _eventService;
    private readonly IVehicleConfigService _vehicleConfigService;
    private readonly ILocationService _locationService;

    private MenuListItem _changeLanguageListItem;

    public VehicleSelectorMenu(
        IConfigService configService, 
        IEntityService entityService,
        IEventService eventService, 
        IVehicleConfigService vehicleConfigService,
        ILocationService locationService)
        : base(Controls.VehicleSelectorMenu.Id, Controls.VehicleSelectorMenu.Title, Controls.VehicleSelectorMenu.Key)
    {
        this._configService = configService;
        this._entityService = entityService;
        this._eventService = eventService;
        this._vehicleConfigService = vehicleConfigService;
        this._locationService = locationService;
    }

    public MenuListItem VehicleSpawnerItem { get; private set; }

    public override bool CanOpen(FPlayer player)
    {
        var config = this._configService.GeneralConfig.Menu.VehicleSelector;
        if (config.AccessEverywhere())
        {
            return true;
        }

        var playerIsInVehicle = player.FPed.Properties.IsInVehicle;
        if (config.AccessOnlyVehicle() && playerIsInVehicle)
        {
            return true;
        }

        var locations = this._locationService.GetAll(LocationType.VehicleSelector);
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

        this.VehicleSpawnerItem = new MenuListItem(
            "UI.MENU.VEHICLE.SPAWN.TITLE",
            "UI.MENU.VEHICLE.SPAWN.DESCRIPTION",
            this._vehicleConfigService.GetAll(VehicleConfigType.Police).Select(vehicle => new MenuListOption(vehicle.Name)).ToList());
        this.VehicleSpawnerItem.OnSelect += async (player, index) =>
        {
            var lastVehicle = Cfx.API.GetVehiclePedIsIn(player.GetUnderlyingPlayer().Character.Handle, false);
            if (lastVehicle != 0)
            {
                Cfx.API.DeleteEntity(lastVehicle);
            }

            var policeVehicle = this._vehicleConfigService.GetAll(VehicleConfigType.Police).ElementAt(index);
            var spawnedVehicle = await this._entityService.SpawnVehicleAsync((uint)Cfx.API.GetHashKey(policeVehicle.Model),
                player.GetUnderlyingPlayer().Character.Position, player.GetUnderlyingPlayer().Character.Heading);
            Cfx.API.SetPedIntoVehicle(player.GetUnderlyingPlayer().Character.Handle, spawnedVehicle.GetUnderlyingVehicle.Handle, -1);
            this._eventService.TriggerEventStrictWithoutCallback(
                player.GetUnderlyingPlayer(),
                Events.Vehicle.SetCosmetics,
                JsonConvert.SerializeObject(new FVehiclePropertiesDTO.SetCosmetics
                {
                    NetworkId = spawnedVehicle.NetworkId,
                    Livery = policeVehicle.Livery,
                    Extras = policeVehicle.Extras.ToList(),
                }));
        };
        items.Add(this.VehicleSpawnerItem);

        return items;
    }
}