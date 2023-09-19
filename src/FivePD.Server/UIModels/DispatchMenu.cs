using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.ConfigModels.GeneralConfigs;
using FivePD.Common.Enums;
using FivePD.Common.Utilities;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Interfaces.Callouts;
using FivePD.Server.Models;
using FivePD.Server.Models.AssistanceServiceModels;
using FivePD.Server.UIModels.MenuModels;

namespace FivePD.Server.UIModels;

#pragma warning disable

public sealed class DispatchMenu : Menu
{
    private readonly ICalloutManagerService _calloutManagerService;
    private readonly IAssistanceServiceManagerService _assistanceService;
    private readonly IConfigService _configService;
    private readonly IBackupManagerService _backupManagerService;
    private readonly IEntityService _entityService;
    private readonly INotificationService _notificationService;
    private readonly IMenuService _menuService;

    public DispatchMenu(
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

    internal MenuButtonItem CompleteIncident { get; private set; }

    internal MenuButtonItem ToggleAmbulance { get; private set; }

    internal MenuButtonItem ToggleTowTruck { get; private set; }

    private Guid _requestedAmbulanceId = Guid.Empty;

    private Guid _requestedTowTruckId = Guid.Empty;

    protected override List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        this.CompleteIncident = new MenuButtonItem("test complete incident", string.Empty);
        this.CompleteIncident.OnSelect += async (player) => { this._calloutManagerService.CompleteCalloutOfPlayer(player); };
        items.Add(this.CompleteIncident);

        this.ToggleAmbulance = new MenuButtonItem("Call ambulance", string.Empty);
        this.ToggleAmbulance.OnSelect += async player =>
        {
            if (this._requestedAmbulanceId == Guid.Empty)
            {
                this._requestedAmbulanceId = this._assistanceService.Call<Ambulance>(player);
                this._menuService.UpdateMenuItem<DispatchMenu>(player, menu => menu.ToggleAmbulance, item => item.Title, "Cancel ambulance");
            }
            else
            {
                this._assistanceService.Cancel(this._requestedAmbulanceId);
                this._requestedAmbulanceId = Guid.Empty;
                this._menuService.UpdateMenuItem<DispatchMenu>(player, menu => menu.ToggleAmbulance, item => item.Title, "Call ambulance");
            }
        };
        items.Add(this.ToggleAmbulance);

        this.ToggleTowTruck = new MenuButtonItem("Call tow truck", string.Empty);
        this.ToggleTowTruck.OnSelect += async (player) =>
        {
            if (this._requestedTowTruckId == Guid.Empty)
            {
                this._requestedTowTruckId = this._assistanceService.Call<TowTruck>(player);
                this._menuService.UpdateMenuItem<DispatchMenu>(player, menu => menu.ToggleTowTruck, item => item.Title, "Cancel tow truck");
            }
            else
            {
                this._assistanceService.Cancel(this._requestedTowTruckId);
                this._requestedTowTruckId = Guid.Empty;
                this._menuService.UpdateMenuItem<DispatchMenu>(player, menu => menu.ToggleTowTruck, item => item.Title, "Call tow truck");
            }
        };
        items.Add(this.ToggleTowTruck);

        foreach (var item in this._configService.GeneralConfig.DispatchActions)
        {
            if (item.OnSelect is null)
            {
                continue;
            }

            var buttonItem = new MenuButtonItem(item.Title, item.Description);
            var actions = new List<Func<FPlayer, Task>>();

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.SetPlayerOnDuty))
            {
                actions.Add(async player => { player.DutyStatus.Value = FDutyStatus.OnDuty; });
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.SetPlayerOffDuty))
            {
                actions.Add(async player => { player.DutyStatus.Value = FDutyStatus.OffDuty; });
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.SetPlayerCanReceiveCallouts))
            {
                actions.Add(async player => player.Unit.Availability.Value = FUnitAvailability.Receiving);
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.SetPlayerCanNotReceiveCallouts))
            {
                actions.Add(async player => player.Unit.Availability.Value = FUnitAvailability.NotReceiving);
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.CompleteCurrentCallout))
            {
                actions.Add(async player => this._calloutManagerService.CompleteCalloutOfPlayer(player));
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.TriggerPanicMode))
            {
                // TODO
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.RequestBackup))
            {
                actions.Add(async player => this._backupManagerService.TriggerBackupRequest(player.Unit));
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.SetAsStatus))
            {
                actions.Add(async player =>
                {
                    var unit = player.Unit;
                    if (unit is not null)
                    {
                        unit.Status.Value = item.Title;
                    }
                });
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.RequestIDCheck))
            {
                actions.Add(async player => this.RequestIdCheck(player));
            }

            buttonItem.OnSelect += async (player) =>
            {
                foreach (var action in actions)
                {
                    action.Invoke(player);
                }
            };
            items.Add(buttonItem);
        }

        return items;
    }

    private async Task RequestIdCheck(FPlayer player)
    {
        var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
        if (fPed is null)
        {
            return;
        }

        if (!fPed.HasGeneratedData.Value)
        {
            return;
        }

        var license = fPed.GetLicense(PedLicenseType.Driving);
        if (license is null)
        {
            this._notificationService.ShowNotification(player, "no driving license");
            return;
        }

        var text = new GtaStyleText()
            .Append(fPed.Firstname + " " + fPed.Lastname)
            .NewLine();

        if (fPed.Warrants.Count == 0)
        {
            text.Append("No warrants");
        }
        else
        {
            text.Red($"{fPed.Warrants.Count} warrant(s)");
        }

        this._notificationService.ShowNotificationWithTexture(
            player,
            "CHAR_CALL911",
            "LABEL.DISPATCH",
            "ID Check",
            text.Build());
    }
}