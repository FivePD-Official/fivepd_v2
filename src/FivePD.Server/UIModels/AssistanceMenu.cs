using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
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

    internal MenuListItem RequestService { get; private set; }

    private Guid _requestedAmbulanceId = Guid.Empty;

    private Guid _requestedTowingId = Guid.Empty;

    protected override List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        this.RequestService = new MenuListItem("Request", "", new List<MenuListOption>
        {
            new("Ambulance"),
            new("Towing"),
        });
        this.RequestService.OnSelect += async (player, index) =>
        {
            if (index == 0)
            {
                var cancelAmbulanceItem = new MenuButtonItem("Cancel ambulance", string.Empty);
                cancelAmbulanceItem.OnSelect += _ =>
                {
                    OnCancel();
                    this._assistanceService.Cancel(ref this._requestedAmbulanceId);
                    return Task.CompletedTask;
                };

                void OnCancel()
                {
                    this._menuService.UpdateMenuListOption<AssistanceMenu, bool>(player, m => m.RequestService, index, item => item.Enabled, true);
                    this._menuService.RemoveMenuItem<AssistanceMenu>(player, cancelAmbulanceItem);
                }

                this._requestedAmbulanceId = this._assistanceService.Call<Ambulance>(player, OnCancel);
                this._menuService.AddMenuItem<AssistanceMenu>(player, cancelAmbulanceItem);
            }
            else if (index == 1)
            {
                var cancelTowingItem = new MenuButtonItem("Cancel towing", string.Empty);
                cancelTowingItem.OnSelect += _ =>
                {
                    OnCancel();
                    this._assistanceService.Cancel(ref this._requestedTowingId);
                    return Task.CompletedTask;
                };
                
                void OnCancel()
                {
                    this._menuService.UpdateMenuListOption<AssistanceMenu, bool>(player, m => m.RequestService, index, item => item.Enabled, true);
                    this._menuService.RemoveMenuItem<AssistanceMenu>(player, cancelTowingItem);
                }

                this._requestedTowingId = this._assistanceService.Call<Towing>(player, OnCancel);
                this._menuService.AddMenuItem<AssistanceMenu>(player, cancelTowingItem);
            }

            this._menuService.UpdateMenuListOption<AssistanceMenu, bool>(player, m => m.RequestService, index, item => item.Enabled, false);
        };
        items.Add(this.RequestService);

        return items;
    }
}