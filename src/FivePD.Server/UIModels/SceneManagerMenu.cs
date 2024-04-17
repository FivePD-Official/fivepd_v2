using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FivePD.Common;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.UIModels.MenuModels;

namespace FivePD.Server.UIModels;

#pragma warning disable

public class SceneManagerMenu : Menu
{
    private readonly IConfigService _configService;
    private readonly ISceneManagementService _sceneManagementService;
    
    private int _selectedSpeedZoneSpeed;
    private int _selectedSpeedZoneRadius;
    private Guid _createdSpeedZoneId = Guid.Empty;

    public SceneManagerMenu(
        IConfigService configService,
        ISceneManagementService sceneManagementService)
        : base(Controls.SceneManagerMenu.Id, Controls.SceneManagerMenu.Title, Controls.SceneManagerMenu.Key)
    {
        this._configService = configService;
        this._sceneManagementService = sceneManagementService;
    }

    public override bool CanOpen(FPlayer player)
    {
        return true;
    }

    protected override List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        if (this._configService.GeneralConfig.Menu.SceneManager.SpeedZone.Enabled)
        {
            items.AddRange(this.CreateSpeedZoneItems());
        }

        if (this._configService.GeneralConfig.Menu.SceneManager.ObjectPlacement.Enabled)
        {
            items.AddRange(this.CreateObjectPlacementItems());
        }

        return items;
    }

    private List<MenuItem> CreateSpeedZoneItems()
    {
        var items = new List<MenuItem> { new MenuDividerItem("Speed zone") };
        
        var speeds = this._configService.GeneralConfig.Menu.SceneManager.SpeedZone.Speed
            .Select(x => new MenuListOption(x.ToString()))
            .ToList();

        var speedSelector = new MenuListItem("Speed", string.Empty, speeds);
        speedSelector.OnSelect += (_, index) =>
        {
            this._selectedSpeedZoneSpeed = this._configService.GeneralConfig.Menu.SceneManager.SpeedZone.Speed[index];
            return Task.CompletedTask;
        };
        items.Add(speedSelector);

        var radiuses = this._configService.GeneralConfig.Menu.SceneManager.SpeedZone.Radius
            .Select(x => new MenuListOption(x.ToString()))
            .ToList();
        var radiusSelector = new MenuListItem("Radius", string.Empty, radiuses);
        radiusSelector.OnSelect += (_, index) =>
        {
            this._selectedSpeedZoneRadius = this._configService.GeneralConfig.Menu.SceneManager.SpeedZone.Radius[index];
            return Task.CompletedTask;
        };
        items.Add(radiusSelector);

        var createSpeedZone = new MenuButtonItem("Create speed zone", string.Empty);
        createSpeedZone.OnSelect += player =>
        {
            this._createdSpeedZoneId = this._sceneManagementService.CreateSpeedZone(player.GetUnderlyingPlayer().Character.Position, this._selectedSpeedZoneRadius, this._selectedSpeedZoneSpeed);
            return Task.CompletedTask;
        };
        items.Add(createSpeedZone);

        var deleteSpeedZone = new MenuButtonItem("Delete speed zone", string.Empty, false);
        deleteSpeedZone.OnSelect += _ =>
        {
            this._sceneManagementService.DeleteSpeedZone(this._createdSpeedZoneId);
            return Task.CompletedTask;
        };
        items.Add(deleteSpeedZone);

        var releaseTraffic = new MenuButtonItem("Release traffic", string.Empty, false);
        releaseTraffic.OnSelect += (player) => { return Task.CompletedTask; };
        items.Add(releaseTraffic);

        return items;
    }

    private List<MenuItem> CreateObjectPlacementItems()
    {
        var items = new List<MenuItem> { new MenuDividerItem("Object placement") };

        var speeds = this._configService.GeneralConfig.Menu.SceneManager.ObjectPlacement.Objects
            .Select(x => new MenuListOption(x.Name.ToString()))
            .ToList();
        var speedSelector = new MenuListItem("Spawn object", string.Empty, speeds);
        speedSelector.OnSelect += (player, index) => { return Task.CompletedTask; };
        items.Add(speedSelector);

        var place = new MenuButtonItem("Delete all objects", string.Empty, false);
        place.OnSelect += (player) => { return Task.CompletedTask; };
        items.Add(place);

        var remove = new MenuButtonItem("Delete previous object", string.Empty, false);
        remove.OnSelect += (player) => { return Task.CompletedTask; };
        items.Add(remove);

        return items;
    }
}