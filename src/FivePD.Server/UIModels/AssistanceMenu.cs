using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
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
    private readonly IAssistanceServiceManagerService _assistanceServiceManagerService;
    private readonly IConfigService _configService;
    private readonly IMenuService _menuService;

    public AssistanceMenu(
        IAssistanceServiceManagerService assistanceServiceManagerService,
        IConfigService configService,
        IMenuService menuService)
        : base(Controls.AssistanceMenu.Id, Controls.AssistanceMenu.Title, Controls.AssistanceMenu.Key)
    {
        this._assistanceServiceManagerService = assistanceServiceManagerService;
        this._configService = configService;
        this._menuService = menuService;
    }

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

        foreach (var service in this._assistanceServiceManagerService.AvailableServices)
        {
            var button = new MenuButtonItem(service.Options.RequestLabel, service.Options.Description);
            var id = Guid.Empty;
            button.OnSelect += async player =>
            {
                if (id == Guid.Empty)
                {
                    id = await this._assistanceServiceManagerService.CallAsync(service.Type, player, OnCancel);
                    this._menuService.UpdateMenuItem<AssistanceMenu, string>(player, button, item => item.Title, service.Options.CancelLabel);
                }
                else
                {
                    this._assistanceServiceManagerService.Cancel(ref id);
                }

                void OnCancel()
                {
                    this._menuService.UpdateMenuItem<AssistanceMenu, string>(player, button, item => item.Title, service.Options.RequestLabel);
                    id = Guid.Empty;
                }
            };
            items.Add(button);
        }

        return items;
    }
}