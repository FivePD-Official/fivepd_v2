using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.ConfigModels.GeneralConfigs;
using FivePD.Common.Enums;
using FivePD.Server.EntityWrappers;
using FivePD.Server.Interfaces;
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

    public DispatchMenu(
        ICalloutManagerService calloutManagerService,
        IAssistanceServiceManagerService assistanceServiceManagerService,
        IConfigService configService,
        IBackupManagerService backupManagerService)
        : base(Controls.AssistanceMenu.Id, Controls.AssistanceMenu.Title, Controls.AssistanceMenu.Key, true)
    {
        this._calloutManagerService = calloutManagerService;
        this._assistanceService = assistanceServiceManagerService;
        this._configService = configService;
        this._backupManagerService = backupManagerService;

        this.Items = this.CreateItems();
    }

    public MenuButtonItem CompleteIncident { get; private set; }

    public MenuButtonItem CallAmbulance { get; private set; }

    private List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        this.CompleteIncident = new MenuButtonItem("test complete incident", string.Empty);
        this.CompleteIncident.OnSelect += player => { this._calloutManagerService.CompleteCalloutOfPlayer(player); };
        items.Add(this.CompleteIncident);

        this.CallAmbulance = new MenuButtonItem("test call ambulance", string.Empty);
        this.CallAmbulance.OnSelect += player =>
        {
            this._assistanceService.Call<Ambulance>(player);
        };
        items.Add(this.CallAmbulance);
        
        var t = new MenuButtonItem("test call tow truck", string.Empty);
        t.OnSelect += player =>
        {
            this._assistanceService.Call<TowTruck>(player);
        };
        items.Add(t);

        foreach (var status in this._configService.GeneralConfig.DispatchActions)
        {
            var item = new MenuButtonItem(status.Title, status.Description);
            var actions = new List<Action<FPlayer>>();

            if (status.OnSelect is not null)
            {
                if (status.OnSelect.Contains(DispatchActionOnSelectKeys.SetPlayerOnDuty))
                {
                    actions.Add(player =>
                    {
                        player.DutyStatus.Value = FDutyStatus.OnDuty;
                    });
                }
                
                if (status.OnSelect.Contains(DispatchActionOnSelectKeys.SetPlayerOffDuty))
                {
                    actions.Add(player =>
                    {
                        player.DutyStatus.Value = FDutyStatus.OffDuty;
                    });
                }

                if (status.OnSelect.Contains(DispatchActionOnSelectKeys.SetPlayerCanReceiveCallouts))
                {
                    actions.Add(player => player.Unit.Availability.Value = FUnitAvailability.Receiving);
                }

                if (status.OnSelect.Contains(DispatchActionOnSelectKeys.SetPlayerCanNotReceiveCallouts))
                {
                    actions.Add(player => player.Unit.Availability.Value = FUnitAvailability.NotReceiving);
                }

                if (status.OnSelect.Contains(DispatchActionOnSelectKeys.CompleteCurrentCallout))
                {
                    actions.Add(player => this._calloutManagerService.CompleteCalloutOfPlayer(player));
                }
                
                if (status.OnSelect.Contains(DispatchActionOnSelectKeys.TriggerPanicMode))
                {
                    // TODO
                }
                
                if (status.OnSelect.Contains(DispatchActionOnSelectKeys.RequestBackup))
                {
                    actions.Add(player => this._backupManagerService.TriggerBackupRequest(player.Unit));
                }
                
                if (status.OnSelect.Contains(DispatchActionOnSelectKeys.SetStatus))
                {
                    actions.Add(player =>
                    {
                        var unit = player.Unit;
                        if (unit is not null)
                        {
                            unit.Status.Value = status.Title;
                        }
                    });
                }
            }

            item.OnSelect += player =>
            {
                foreach (var action in actions)
                {
                    action.Invoke(player);
                }
            };

            items.Add(item);
        }

        return items;
    }
}