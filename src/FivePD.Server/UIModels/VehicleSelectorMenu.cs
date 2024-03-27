using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.ConfigModels.MenuConfigs;
using FivePD.Common.DTO;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
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
    private readonly DutyMenuConfig _dutyMenuConfig;
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
        this._dutyMenuConfig = configService.MenuConfig.DutyMenu;
        this._entityService = entityService;
        this._eventService = eventService;
        this._vehicleConfigService = vehicleConfigService;
        this._locationService = locationService;
    }

    public MenuListItem VehicleSpawnerItem { get; private set; }

    public override bool CanOpen(FPlayer player)
    {
        var locations = this._locationService.GetAll(LocationType.VehicleSelector);
        return locations.Any(x => player.GetUnderlyingPlayer().Character.Position.DistanceToSquared(x.Position) <= 3f);
    }

    protected override List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        var policeVehicles = this._vehicleConfigService.GetAll(VehicleConfigType.Police); // TODO: won't refresh when vehicle configs are modified!

        if (this._dutyMenuConfig.SpawnVehicle.Enabled)
        {
            this.VehicleSpawnerItem = new MenuListItem(
                "DUTY_MENU.ITEM.SPAWN_VEHICLE.TITLE",
                "DUTY_MENU.ITEM.SPAWN_VEHICLE.DESCRIPTION",
                policeVehicles.Select(vehicle => new MenuListOption(vehicle.Name)).ToList());
            this.VehicleSpawnerItem.OnSelect += async (player, index) =>
            {
                var lastVehicle = Cfx.API.GetVehiclePedIsIn(player.GetUnderlyingPlayer().Character.Handle, false);
                if (lastVehicle != 0)
                {
                    Cfx.API.DeleteEntity(lastVehicle);
                }

                var policeVehicle = policeVehicles.ElementAt(index);
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
        }

        return items;
    }
}