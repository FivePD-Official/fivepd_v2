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
    private readonly IMenuService _menuService;
    
    private int _selectedSpeedZoneSpeed;
    private int _selectedSpeedZoneRadius;
    private Guid _createdSpeedZoneId = Guid.Empty;

    public SceneManagerMenu(
        IConfigService configService,
        ISceneManagementService sceneManagementService,
        IMenuService menuService)
        : base(Controls.SceneManagerMenu.Id, Controls.SceneManagerMenu.Title, Controls.SceneManagerMenu.Key)
    {
        this._configService = configService;
        this._sceneManagementService = sceneManagementService;
        this._menuService = menuService;
    }
    
    internal MenuButtonItem CreateSpeedZone { get; private set; }

    internal MenuButtonItem DeleteSpeedZone { get; private set; }

    internal MenuButtonItem ReleaseTraffic { get; private set; }

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
        var items = new List<MenuItem> { new MenuDividerItem("UI.MENU.SCENE_MANAGER.SPEED_ZONE.TITLE") };

        var speedList = this._configService.GeneralConfig.Menu.SceneManager.SpeedZone.Speed;
        var radiusList = this._configService.GeneralConfig.Menu.SceneManager.SpeedZone.Radius;
        
        this._selectedSpeedZoneSpeed = speedList.Count > 0 ? speedList[0] : 0;
        this._selectedSpeedZoneRadius = radiusList.Count > 0 ? radiusList[0] : 0;

        var speedSelector = new MenuListItem("UI.MENU.SCENE_MANAGER.SPEED_ZONE.SPEED.TITLE", "UI.MENU.SCENE_MANAGER.SPEED_ZONE.SPEED.DESCRIPTION", speedList.Select(x => new MenuListOption(x.ToString())).ToList());
        speedSelector.OnIndexChange += (_, index) =>
        {
            this._selectedSpeedZoneSpeed = speedList[index];
            return Task.CompletedTask;
        };
        items.Add(speedSelector);

        var radiusSelector = new MenuListItem("UI.MENU.SCENE_MANAGER.SPEED_ZONE.RADIUS.TITLE", "UI.MENU.SCENE_MANAGER.SPEED_ZONE.RADIUS.DESCRIPTION", radiusList.Select(x => new MenuListOption(x.ToString())).ToList());
        radiusSelector.OnIndexChange += (_, index) =>
        {
            this._selectedSpeedZoneRadius = radiusList[index];
            return Task.CompletedTask;
        };
        items.Add(radiusSelector);

        this.CreateSpeedZone = new MenuButtonItem("UI.MENU.SCENE_MANAGER.SPEED_ZONE.CREATE.TITLE", "UI.MENU.SCENE_MANAGER.SPEED_ZONE.CREATE.DESCRIPTION");
        this.CreateSpeedZone.OnSelect += player =>
        {
            this._createdSpeedZoneId = this._sceneManagementService.CreateSpeedZone(player.GetUnderlyingPlayer().Character.Position, this._selectedSpeedZoneRadius, this._selectedSpeedZoneSpeed);
            this._menuService.UpdateMenuItem<SceneManagerMenu, bool>(player, m => m.CreateSpeedZone, i => i.Enabled, false);
            this._menuService.UpdateMenuItem<SceneManagerMenu, bool>(player, m => m.DeleteSpeedZone, i => i.Enabled, true);
            this._menuService.UpdateMenuItem<SceneManagerMenu, bool>(player, m => m.ReleaseTraffic, i => i.Enabled, true);
            return Task.CompletedTask;
        };
        items.Add(this.CreateSpeedZone);

        this.DeleteSpeedZone = new MenuButtonItem("UI.MENU.SCENE_MANAGER.SPEED_ZONE.DELETE.TITLE", "UI.MENU.SCENE_MANAGER.SPEED_ZONE.DELETE.DESCRIPTION", false);
        this.DeleteSpeedZone.OnSelect += player =>
        {
            var deleted = this._sceneManagementService.DeleteSpeedZone(this._createdSpeedZoneId);
            if (deleted)
            {
                this._menuService.UpdateMenuItem<SceneManagerMenu, bool>(player, m => m.CreateSpeedZone, i => i.Enabled, true);
                this._menuService.UpdateMenuItem<SceneManagerMenu, bool>(player, m => m.DeleteSpeedZone, i => i.Enabled, false);
                this._menuService.UpdateMenuItem<SceneManagerMenu, bool>(player, m => m.ReleaseTraffic, i => i.Enabled, false);
                this._createdSpeedZoneId = Guid.Empty;
            }
            return Task.CompletedTask;
        };
        items.Add(this.DeleteSpeedZone);

        this.ReleaseTraffic = new MenuButtonItem("UI.MENU.SCENE_MANAGER.SPEED_ZONE.RELEASE.TITLE", "UI.MENU.SCENE_MANAGER.SPEED_ZONE.RELEASE.DESCRIPTION", false);
        this.ReleaseTraffic.OnSelect += _ =>
        {
            this._sceneManagementService.ReleaseSpeedZoneTraffic(this._createdSpeedZoneId);
            return Task.CompletedTask;
        };
        items.Add(this.ReleaseTraffic);

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