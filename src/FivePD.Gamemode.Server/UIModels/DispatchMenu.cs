using System;
using System.Collections.Generic;
using CitizenFX.Core;
using FivePD.Gamemode.Common;
using FivePD.Gamemode.Common.ConfigModels;
using FivePD.Gamemode.Common.Enums;
using FivePD.Gamemode.Server.EntityWrappers;
using FivePD.Gamemode.Server.Interfaces;
using FivePD.Gamemode.Server.Models.AssistanceServiceModels;
using FivePD.Gamemode.Server.UIModels.MenuModels;

namespace FivePD.Gamemode.Server.UIModels;

#pragma warning disable

public sealed class DispatchMenu : Menu
{
    private readonly ICalloutManagerService _calloutManagerService;
    private readonly IAssistanceServiceManagerService _assistanceService;
    private readonly IConfigService _configService;

    public DispatchMenu(
        ICalloutManagerService calloutManagerService,
        IAssistanceServiceManagerService assistanceServiceManagerService,
        IConfigService configService)
        : base(Controls.AssistanceMenu.Id, Controls.AssistanceMenu.Title, Controls.AssistanceMenu.Key, true)
    {
        this._calloutManagerService = calloutManagerService;
        this._assistanceService = assistanceServiceManagerService;
        this._configService = configService;

        this.Items = this.CreateItems();
    }

    public MenuButtonItem CompleteIncident { get; private set; }

    public MenuButtonItem CallAmbulance { get; private set; }

    private List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        this.CompleteIncident = new MenuButtonItem("Complete incident", string.Empty);
        this.CompleteIncident.OnSelect += player => { this._calloutManagerService.CompleteCalloutOfPlayer(player); };
        items.Add(this.CompleteIncident);

        this.CallAmbulance = new MenuButtonItem("Call ambulance", string.Empty);
        this.CallAmbulance.OnSelect += player =>
        {
            this._assistanceService.Call<Ambulance>(new Vector3(704.09f, -117.47f, 52.83f), player.GetUnderlyingPlayer().Character.Position);
        };

        foreach (var status in this._configService.GeneralConfig.Status)
        {
            var item = new MenuButtonItem(status.Title, status.Description);
            if (status.OnSelect is null)
            {
                items.Add(item);
                continue;
            }

            var actions = new List<Action<FPlayer>>();
            if (status.OnSelect.Contains(StatusOnSelectKeys.SetsPlayerCanReceiveCallouts))
            {
                actions.Add(player => player.Availability.Value = FPlayerAvailability.Receiving);
            }

            if (status.OnSelect.Contains(StatusOnSelectKeys.SetsPlayerCanNotReceiveCallouts))
            {
                actions.Add(player => player.Availability.Value = FPlayerAvailability.NotReceiving);
            }

            if (status.OnSelect.Contains(StatusOnSelectKeys.CompletesCurrentCallout))
            {
                actions.Add(player => this._calloutManagerService.CompleteCalloutOfPlayer(player));
            }
            
            if (status.OnSelect.Contains(StatusOnSelectKeys.TriggersPanicMode))
            {
                // TODO
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