using System;
using System.Collections.Generic;
using System.Text;
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
using Microsoft.Extensions.Primitives;

namespace FivePD.Server.UIModels;

#pragma warning disable

public sealed class DispatchMenu : Menu
{
    private readonly IIncidentService _incidentService;
    private readonly IConfigService _configService;
    private readonly IBackupManagerService _backupManagerService;
    private readonly IEntityService _entityService;
    private readonly IUnitManagerService _unitManagerService;
    private readonly IGameNotificationService _gameNotificationService;
    private readonly IPedStorageService _pedStorageService;
    private readonly IVehicleStorageService _vehicleStorageService;

    public DispatchMenu(
        IIncidentService incidentService,
        IConfigService configService,
        IBackupManagerService backupManagerService,
        IEntityService entityService,
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
                        this.DutyNotification(player, "LABEL.MENU.DISPATCH.ON_DUTY.NOTIFICATION");
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
                        this.DutyNotification(player, "LABEL.MENU.DISPATCH.OFF_DUTY.NOTIFICATION");
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
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "ERROR.UNEXPECTED",
                });
            return;
        }

        if (!fPed.HasGeneratedData.Value)
        {
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "ERROR.UNEXPECTED",
                });
            return;
        }

        var pedData = this._pedStorageService.GetPedData(fPed);
        if (pedData is null)
        {
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "ERROR.UNEXPECTED",
                });
            return;
        }

        var license = pedData.GetLicense(PedLicenseType.Driving);
        if (license is null)
        {
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "No driving license available.",
                });
            return;
        }

        var text = new StringBuilder()
            .Append(pedData.Firstname + " " + pedData.Lastname)
            .Append("{{NEW_LINE}}");

        if (pedData.Warrants.Count == 0)
        {
            text.Append("No warrants");
        }
        else
        {
            text.Append($"{pedData.Warrants.Count} warrant(s)");
        }

        this._gameNotificationService.Notify(
            player,
            new GameNotificationOptions
            {
                Title = "FivePD - ID check",
                Message = text.ToString(),
            });
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
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "ERROR.UNEXPECTED",
                });
            return;
        }

        if (!fVehicle.HasGeneratedData.Value)
        {
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "ERROR.UNEXPECTED",
                });
            return;
        }

        var vehicleData = this._vehicleStorageService.GetVehicleData(fVehicle);
        if (vehicleData is null)
        {
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "ERROR.UNEXPECTED",
                });
            return;
        }

        var text = new StringBuilder()
            .Append(vehicleData.LicensePlate)
            .Append("{{NEW_LINE}}");

        var registration = vehicleData.GetDocument(VehicleDocumentType.Registration);
        if (registration is not null)
        {
            text.Append("Registration: ");
            if (registration.Status == VehicleDocumentStatus.Valid)
            {
                text.Append("Valid").Append("{{NEW_LINE}}");
            }
            else
            {
                text.Append(registration.Status switch
                {
                    VehicleDocumentStatus.Expired => "Expired",
                    VehicleDocumentStatus.Suspended => "Suspended",
                    VehicleDocumentStatus.Revoked => "Revoked",
                    _ => "Unknown",
                }).Append("{{NEW_LINE}}");
            }
        }

        var insurance = vehicleData.GetDocument(VehicleDocumentType.Insurance);
        if (insurance is not null)
        {
            text.Append("Insurance: ");
            if (insurance.Status == VehicleDocumentStatus.Valid)
            {
                text.Append("Valid");
            }
            else
            {
                text.Append(insurance.Status switch
                {
                    VehicleDocumentStatus.Expired => "Expired",
                    VehicleDocumentStatus.Suspended => "Suspended",
                    VehicleDocumentStatus.Revoked => "Revoked",
                    _ => "Unknown",
                });
            }
        }

        this._gameNotificationService.Notify(
            player,
            new GameNotificationOptions
            {
                Title = "FivePD - License plate check",
                Message = text.ToString(),
            });
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