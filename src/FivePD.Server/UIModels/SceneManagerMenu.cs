using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Models;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.UIModels.MenuModels;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.UIModels;

#pragma warning disable

public class SceneManagerMenu : Menu
{
    private readonly IConfigService _configService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly ISceneManagementService _sceneManagementService;
    private readonly IMenuService _menuService;
    private readonly IEntityService _entityService;
    private readonly IVector3Service _vector3Service;
    private readonly ICfxService _cfxService;

    private FPlayer _currentPlayer;

    private int _selectedSpeedZoneSpeed;
    private int _selectedSpeedZoneRadius;
    private Guid _createdSpeedZoneId = Guid.Empty;

    private List<Prop> _props = new();

    public SceneManagerMenu(
        IConfigService configService,
        IGameNotificationService gameNotificationService,
        ISceneManagementService sceneManagementService,
        IMenuService menuService,
        IEntityService entityService,
        IVector3Service vector3Service,
        ICfxService cfxService)
        : base(Controls.SceneManagerMenu.Id, Controls.SceneManagerMenu.Title, Controls.SceneManagerMenu.Key)
    {
        this._configService = configService;
        this._gameNotificationService = gameNotificationService;
        this._sceneManagementService = sceneManagementService;
        this._menuService = menuService;
        this._entityService = entityService;
        this._vector3Service = vector3Service;
        this._cfxService = cfxService;

        sceneManagementService.SpeedZoneDeleted += (_, args) =>
        {
            if (this._createdSpeedZoneId == args.Id)
            {
                this.OnDeleteSpeedZone(this._currentPlayer);
            }
        };
    }

    internal MenuButtonItem CreateSpeedZone { get; private set; }

    internal MenuButtonItem DeleteSpeedZone { get; private set; }

    internal MenuButtonItem ReleaseTraffic { get; private set; }

    public override bool CanOpen(FPlayer player)
    {
        var config = this._configService.GeneralConfig.Menu.SceneManager;
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

        return false;
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
        var items = new List<MenuItem> { new MenuDividerItem("LABEL.MENU.SCENE_MANAGER.SPEED_ZONE.TITLE") };

        var speedList = this._configService.GeneralConfig.Menu.SceneManager.SpeedZone.Speed;
        var radiusList = this._configService.GeneralConfig.Menu.SceneManager.SpeedZone.Radius;

        this._selectedSpeedZoneSpeed = speedList.Count > 0 ? speedList[0] : 0;
        this._selectedSpeedZoneRadius = radiusList.Count > 0 ? radiusList[0] : 0;

        var speedSelector = new MenuListItem(
            "LABEL.MENU.SCENE_MANAGER.SPEED_ZONE.SPEED.TITLE",
            "LABEL.MENU.SCENE_MANAGER.SPEED_ZONE.SPEED.DESCRIPTION",
            speedList.Select(x => new MenuListOption(x.ToString())).ToList());
        speedSelector.OnIndexChange += (_, index) =>
        {
            this._selectedSpeedZoneSpeed = speedList[index];
            return Task.CompletedTask;
        };
        items.Add(speedSelector);

        var radiusSelector = new MenuListItem(
            "LABEL.MENU.SCENE_MANAGER.SPEED_ZONE.RADIUS.TITLE",
            "LABEL.MENU.SCENE_MANAGER.SPEED_ZONE.RADIUS.DESCRIPTION",
            radiusList.Select(x => new MenuListOption(x.ToString())).ToList());
        radiusSelector.OnIndexChange += (_, index) =>
        {
            this._selectedSpeedZoneRadius = radiusList[index];
            return Task.CompletedTask;
        };
        items.Add(radiusSelector);

        this.CreateSpeedZone = new MenuButtonItem("LABEL.MENU.SCENE_MANAGER.SPEED_ZONE.CREATE.TITLE", "LABEL.MENU.SCENE_MANAGER.SPEED_ZONE.CREATE.DESCRIPTION");
        this.CreateSpeedZone.OnSelect += player =>
        {
            this._currentPlayer = player;
            this._createdSpeedZoneId = ((IInternalSceneManagementService)this._sceneManagementService).CreateSpeedZone(
                player,
                player.GetUnderlyingPlayer().Character.Position,
                this._selectedSpeedZoneRadius,
                this._selectedSpeedZoneSpeed);
            this._menuService.UpdateMenuItem<SceneManagerMenu, bool>(player, m => m.CreateSpeedZone, i => i.Enabled, false);
            this._menuService.UpdateMenuItem<SceneManagerMenu, bool>(player, m => m.DeleteSpeedZone, i => i.Enabled, true);
            this._menuService.UpdateMenuItem<SceneManagerMenu, bool>(player, m => m.ReleaseTraffic, i => i.Enabled, true);
            return Task.CompletedTask;
        };
        items.Add(this.CreateSpeedZone);

        this.DeleteSpeedZone = new MenuButtonItem(
            "LABEL.MENU.SCENE_MANAGER.SPEED_ZONE.DELETE.TITLE",
            "LABEL.MENU.SCENE_MANAGER.SPEED_ZONE.DELETE.DESCRIPTION",
            false);
        this.DeleteSpeedZone.OnSelect += player =>
        {
            var deleted = this._sceneManagementService.DeleteSpeedZone(this._createdSpeedZoneId);
            if (deleted)
            {
                this.OnDeleteSpeedZone(player);
            }

            return Task.CompletedTask;
        };
        items.Add(this.DeleteSpeedZone);

        this.ReleaseTraffic = new MenuButtonItem(
            "LABEL.MENU.SCENE_MANAGER.SPEED_ZONE.RELEASE.TITLE",
            "LABEL.MENU.SCENE_MANAGER.SPEED_ZONE.RELEASE.DESCRIPTION",
            false);
        this.ReleaseTraffic.OnSelect += p =>
        {
            Debug.WriteLine(
                $"pos: {Math.Sqrt(this._sceneManagementService.SpeedZones()[0].Position.DistanceToSquared2D(p.GetUnderlyingPlayer().Character.Position))}");
            this._sceneManagementService.ReleaseSpeedZoneTraffic(this._createdSpeedZoneId);
            return Task.CompletedTask;
        };
        items.Add(this.ReleaseTraffic);

        return items;
    }

    private void OnDeleteSpeedZone(FPlayer player)
    {
        this._menuService.UpdateMenuItem<SceneManagerMenu, bool>(player, m => m.CreateSpeedZone, i => i.Enabled, true);
        this._menuService.UpdateMenuItem<SceneManagerMenu, bool>(player, m => m.DeleteSpeedZone, i => i.Enabled, false);
        this._menuService.UpdateMenuItem<SceneManagerMenu, bool>(player, m => m.ReleaseTraffic, i => i.Enabled, false);
        this._createdSpeedZoneId = Guid.Empty;
        this._currentPlayer = null;
    }

    private List<MenuItem> CreateObjectPlacementItems()
    {
        var items = new List<MenuItem> { new MenuDividerItem("LABEL.MENU.SCENE_MANAGER.OBJECT_PLACEMENT.TITLE") };

        var objects = this._configService.GeneralConfig.Menu.SceneManager.ObjectPlacement.Objects;
        var speedSelector = new MenuListItem("LABEL.MENU.SCENE_MANAGER.OBJECT_PLACEMENT.SPAWN.TITLE", string.Empty, objects
            .Select(x => new MenuListOption(x.Name.ToString()))
            .ToList());
        speedSelector.OnSelect += async (player, index) =>
        {
            var forwardVector = await this._vector3Service.GetPlayerForwardVectorAsync(player.GetUnderlyingPlayer());
            var prop = await this._entityService.SpawnPropAsync(
                Cfx.API.GetHashKey(objects[index].Model),
                player.GetUnderlyingPlayer().Character.Position + forwardVector * 2f);
            prop.Heading = player.GetUnderlyingPlayer().Character.Heading;
            this._cfxService.PlaceObjectOnGroundProperly(prop.NetworkId);
            this._props.Add(prop);
        };
        items.Add(speedSelector);

        var place = new MenuButtonItem("LABEL.MENU.SCENE_MANAGER.OBJECT_PLACEMENT.DELETE_ALL.TITLE", string.Empty);
        place.OnSelect += _ =>
        {
            foreach (var prop in this._props)
            {
                Cfx.API.DeleteEntity(prop.Handle);
            }

            this._props.Clear();

            return Task.CompletedTask;
        };
        items.Add(place);

        var remove = new MenuButtonItem("LABEL.MENU.SCENE_MANAGER.OBJECT_PLACEMENT.DELETE_PREVIOUS.TITLE", string.Empty);
        remove.OnSelect += _ =>
        {
            var lastProp = this._props.LastOrDefault();
            if (lastProp is not null)
            {
                Cfx.API.DeleteEntity(lastProp.Handle);
                this._props.Remove(lastProp);
            }

            return Task.CompletedTask;
        };
        items.Add(remove);

        return items;
    }
}