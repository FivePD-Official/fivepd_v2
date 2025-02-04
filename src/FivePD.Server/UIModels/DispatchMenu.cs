using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FivePD.Common;
using FivePD.Common.ConfigModels.GeneralConfigs;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Common.Utilities;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.UIModels.MenuModels;

namespace FivePD.Server.UIModels;

#pragma warning disable

public sealed class DispatchMenu : Menu
{
    private readonly IIncidentService _incidentService;
    private readonly IConfigService _configService;
    private readonly IBackupManagerService _backupManagerService;
    private readonly IEntityService _entityService;
    private readonly INotificationService _notificationService;
    private readonly IUnitManagerService _unitManagerService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly IPedStorageService _pedStorageService;
    private readonly IVehicleStorageService _vehicleStorageService;

    public DispatchMenu(
        IIncidentService incidentService,
        IConfigService configService,
        IBackupManagerService backupManagerService,
        IEntityService entityService,
        INotificationService notificationService,
        IUnitManagerService unitManagerService,
        IGameNotificationService gameNotificationService,
        IPedStorageService pedStorageService,
        IVehicleStorageService vehicleStorageService)
        : base(Controls.DispatchMenu.Id, Controls.DispatchMenu.Title, Controls.DispatchMenu.Key)
    {
        this._incidentService = incidentService;
        this._configService = configService;
        this._backupManagerService = backupManagerService;
        this._entityService = entityService;
        this._notificationService = notificationService;
        this._unitManagerService = unitManagerService;
        this._gameNotificationService = gameNotificationService;
        this._pedStorageService = pedStorageService;
        this._vehicleStorageService = vehicleStorageService;
    }

    protected override List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        foreach (var item in this._configService.GeneralConfig.Menu.Dispatch.Items)
        {
            if (item.OnSelect is null)
            {
                if (item.Divider)
                {
                    items.Add(new MenuDividerItem(item.Title));
                }

                continue;
            }

            var buttonItem = new MenuButtonItem(item.Title, item.Description);
            var actions = new List<Func<FPlayer, Task>>();

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.SetPlayerOnDuty))
            {
                actions.Add(async player =>
                {
                    if (player.Department is not null)
                    {
                        player.DutyStatus.Value = FDutyStatus.OnDuty;
                        this.DutyNotification(player, "UI.MENU.DISPATCH.ON_DUTY.NOTIFICATION");
                    }
                });
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.SetPlayerOffDuty))
            {
                actions.Add(async player =>
                {
                    if (player.Department is not null)
                    {
                        player.DutyStatus.Value = FDutyStatus.OffDuty;
                        this.DutyNotification(player, "UI.MENU.DISPATCH.OFF_DUTY.NOTIFICATION");
                    }
                });
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.SetPlayerCanReceiveCallouts))
            {
                actions.Add(async player => player.Unit.Availability.Value = FUnitAvailability.Receiving);
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.SetPlayerCanNotReceiveCallouts))
            {
                actions.Add(async player => player.Unit.Availability.Value = FUnitAvailability.NotReceiving);
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.CompleteCurrentIncident))
            {
                actions.Add(async player =>
                {
                    this._incidentService.CompleteIncidentOfPlayer(player);
                    var unit = player.Unit;
                    if (unit is not null)
                    {
                        if (unit.PanicMode.Value)
                        {
                            this._unitManagerService.DisablePanicMode(unit);
                        }
                    }
                });
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.EnablePanicMode))
            {
                actions.Add(async player =>
                {
                    var unit = player.Unit;
                    if (unit is null)
                    {
                        return;
                    }

                    if (!unit.PanicMode.Value)
                    {
                        this._unitManagerService.EnablePanicMode(unit);
                    }
                });
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.DisablePanicMode))
            {
                actions.Add(async player =>
                {
                    var unit = player.Unit;
                    if (unit is null)
                    {
                        return;
                    }

                    if (unit.PanicMode.Value)
                    {
                        this._unitManagerService.DisablePanicMode(unit);
                    }
                });
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

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.RequestIdCheck))
            {
                actions.Add(async player => this.RequestIdCheck(player));
            }

            if (item.OnSelect.Contains(DispatchActionOnSelectKeys.RequestLicensePlateCheck))
            {
                actions.Add(async player => this.RequestLicensePlateCheck(player));
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
            this._notificationService.ShowNotification(player, "UI.ERROR.UNEXPECTED");
            return;
        }

        if (!fPed.HasGeneratedData.Value)
        {
            this._notificationService.ShowNotification(player, "UI.ERROR.UNEXPECTED");
            return;
        }

        var pedData = this._pedStorageService.GetPedData(fPed);
        if (pedData is null)
        {
            this._notificationService.ShowNotification(player, "UI.ERROR.UNEXPECTED");
            return;
        }

        var license = pedData.GetLicense(PedLicenseType.Driving);
        if (license is null)
        {
            this._notificationService.ShowNotification(player, "no driving license");
            return;
        }

        var text = new GtaStyleText()
            .Append(pedData.Firstname + " " + pedData.Lastname)
            .NewLine();

        if (pedData.Warrants.Count == 0)
        {
            text.Append("No warrants");
        }
        else
        {
            text.Red($"{pedData.Warrants.Count} warrant(s)");
        }

        this._notificationService.ShowNotificationWithTexture(
            player,
            "CHAR_CALL911",
            "LABEL.DISPATCH",
            "ID Check",
            text.Build());
    }

    public override bool CanOpen(FPlayer player)
    {
        var config = this._configService.GeneralConfig.Menu.Dispatch;
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

    private async Task RequestLicensePlateCheck(FPlayer player)
    {
        var fVehicle = await this._entityService.GetClosestEntityToPlayerAsync<FVehicle>(player.GetUnderlyingPlayer());
        if (fVehicle is null)
        {
            this._notificationService.ShowNotification(player, "UI.ERROR.UNEXPECTED");
            return;
        }

        if (!fVehicle.HasGeneratedData.Value)
        {
            this._notificationService.ShowNotification(player, "UI.ERROR.UNEXPECTED");
            return;
        }

        var vehicleData = this._vehicleStorageService.GetVehicleData(fVehicle);
        if (vehicleData is null)
        {
            this._notificationService.ShowNotification(player, "UI.ERROR.UNEXPECTED");
            return;
        }

        var text = new GtaStyleText()
            .Append(vehicleData.LicensePlate)
            .NewLine();

        var registration = vehicleData.GetDocument(VehicleDocumentType.Registration);
        if (registration is not null)
        {
            text.Append("Registration: ");
            if (registration.Status == VehicleDocumentStatus.Valid)
            {
                text.Green("Valid").White().NewLine();
            }
            else
            {
                text.Yellow(registration.Status switch
                {
                    VehicleDocumentStatus.Expired => "Expired",
                    VehicleDocumentStatus.Suspended => "Suspended",
                    VehicleDocumentStatus.Revoked => "Revoked",
                    _ => "Unknown",
                }).White().NewLine();
            }
        }

        var insurance = vehicleData.GetDocument(VehicleDocumentType.Insurance);
        if (insurance is not null)
        {
            text.Append("Insurance: ");
            if (insurance.Status == VehicleDocumentStatus.Valid)
            {
                text.Green("Valid");
            }
            else
            {
                text.Yellow(insurance.Status switch
                {
                    VehicleDocumentStatus.Expired => "Expired",
                    VehicleDocumentStatus.Suspended => "Suspended",
                    VehicleDocumentStatus.Revoked => "Revoked",
                    _ => "Unknown",
                });
            }
        }

        // TODO: replace with GameNotificationService
        this._notificationService.ShowNotificationWithTexture(
            player,
            "CHAR_CALL911",
            "LABEL.DISPATCH",
            "License plate check",
            text.Build());
    }

    private async Task DutyNotification(FPlayer player, string message)
    {
        this._gameNotificationService.Notify(player, new GameNotificationOptions
        {
            Title = "FivePD",
            Message = message,
            PlayAudio = this._configService.GeneralConfig.Audio.Files.DutyNotification,
        });
    }
}