using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FivePD.Common;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Models.AssistanceServiceModels;
using FivePD.Server.UIModels.MenuModels;

namespace FivePD.Server.UIModels;

#pragma warning disable

public sealed class AssistanceMenu : Menu
{
    private readonly ICalloutManagerService _calloutManagerService;
    private readonly IAssistanceServiceManagerService _assistanceService;
    private readonly IConfigService _configService;
    private readonly IBackupManagerService _backupManagerService;
    private readonly IEntityService _entityService;
    private readonly INotificationService _notificationService;
    private readonly IMenuService _menuService;

    public AssistanceMenu(
        ICalloutManagerService calloutManagerService,
        IAssistanceServiceManagerService assistanceServiceManagerService,
        IConfigService configService,
        IBackupManagerService backupManagerService,
        IEntityService entityService,
        INotificationService notificationService,
        IMenuService menuService)
        : base(Controls.AssistanceMenu.Id, Controls.AssistanceMenu.Title, Controls.AssistanceMenu.Key)
    {
        this._calloutManagerService = calloutManagerService;
        this._assistanceService = assistanceServiceManagerService;
        this._configService = configService;
        this._backupManagerService = backupManagerService;
        this._entityService = entityService;
        this._notificationService = notificationService;
        this._menuService = menuService;
    }

    internal MenuButtonItem AmbulanceButton { get; private set; }

    internal MenuButtonItem TowButton { get; private set; }

    private Guid _requestedAmbulanceId = Guid.Empty;

    private Guid _requestedTowingId = Guid.Empty;

    public override bool CanOpen(FPlayer player)
    {
        var config = this._configService.GeneralConfig.Menu.Assistance;
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

        this.AmbulanceButton = new MenuButtonItem("Request Ambulance", "");
        this.AmbulanceButton.OnSelect += async player =>
        {
            if (this._requestedAmbulanceId == Guid.Empty)
            {
                this._requestedAmbulanceId = await this._assistanceService.CallAsync<Ambulance>(player, OnCancel);
                this._menuService.UpdateMenuItem<AssistanceMenu, string>(player, m => m.AmbulanceButton, item => item.Title, "Cancel Ambulance");
            }
            else
            {
                this._assistanceService.Cancel(ref this._requestedAmbulanceId);
            }

            void OnCancel()
            {
                this._menuService.UpdateMenuItem<AssistanceMenu, string>(player, m => m.AmbulanceButton, item => item.Title, "Request Ambulance");
                this._requestedAmbulanceId = Guid.Empty;
            }

            
        };
        items.Add(this.AmbulanceButton);

        this.TowButton = new MenuButtonItem("Request Tow", "");
        this.TowButton.OnSelect += async player =>
        {
            if (this._requestedTowingId == Guid.Empty)
            {
                this._requestedTowingId = await this._assistanceService.CallAsync<Towing>(player, OnCancel);
                this._menuService.UpdateMenuItem<AssistanceMenu, string>(player, m => m.TowButton, item => item.Title, "Cancel Tow");
            }
            else
            {
                this._assistanceService.Cancel(ref this._requestedTowingId);
            }

            void OnCancel()
            {
                this._menuService.UpdateMenuItem<AssistanceMenu, string>(player, m => m.TowButton, item => item.Title, "Request Tow");
                this._requestedTowingId = Guid.Empty;
            }
        };
        items.Add(this.TowButton);

        return items;
    }
}