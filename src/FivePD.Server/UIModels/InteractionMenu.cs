using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Utilities;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.UIModels.MenuModels;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.UIModels;

#pragma warning disable

public sealed class InteractionMenu : Menu
{
    private readonly INotificationService _notificationService;
    private readonly IEntityService _entityService;
    private readonly IPedService _pedService;

    public InteractionMenu(INotificationService notificationService, IEntityService entityService, IPedService pedService)
        : base(Controls.InteractionMenu.Id, Controls.InteractionMenu.Title, Controls.InteractionMenu.Key, true)
    {
        this._notificationService = notificationService;
        this._entityService = entityService;
        this._pedService = pedService;
    }

    internal MenuButtonItem PatDown { get; private set; }

    internal MenuListItem AskForLicense { get; private set; }

    protected override List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        this.AskForLicense = new MenuListItem("Ask for license", "", new List<string>
        {
            "Driving",
            "Fishing",
            "Hunting",
            "Ccw",
        });
        this.AskForLicense.OnSelect += async (player, index) =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null)
            {
                return;
            }

            if (fPed.Properties.IsDead || !fPed.HasGeneratedData.Value)
            {
                return;
            }

            PedLicenseType licenseType;
            string licenseTypeLabel;

            switch (index)
            {
                case 0:
                {
                    licenseType = PedLicenseType.Driving;
                    licenseTypeLabel = "Driving license";
                    break;
                }
                case 1:
                {
                    licenseType = PedLicenseType.Fishing;
                    licenseTypeLabel = "Fishing license";
                    break;
                }
                case 2:
                {
                    licenseType = PedLicenseType.Hunting;
                    licenseTypeLabel = "Hunting license";
                    break;
                }
                case 3:
                {
                    licenseType = PedLicenseType.Ccw;
                    licenseTypeLabel = "CCW permit";
                    break;
                }
                default: return;
            }

            var license = fPed.GetLicense(licenseType);
            if (license is null)
            {
                this._notificationService.ShowNotification(player, $"no {licenseTypeLabel}");
                return;
            }

            var text = new GtaStyleText()
                .Blue($"[{licenseTypeLabel}]")
                .NewLine()
                .White($"Issue date: {license.IssueDate:MM-dd-yyyy}")
                .NewLine()
                .Append($"Expiration date: {license.ExpirationDate:MM-dd-yyyy}")
                .NewLine()
                .White("Status: ");

            if (license.Valid)
            {
                text.Green("Valid");
            }
            else
            {
                text.Yellow("Expired");
            }

            this._notificationService.ShowNotification(player, text.Build());
        };
        items.Add(this.AskForLicense);

        this.PatDown = new MenuButtonItem("Pat down the ped", "");
        this.PatDown.OnSelect += async (player) =>
        {
            var playerPed = FPed.FromFPlayer(player);

            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null)
            {
                return;
            }

            if (!fPed.HasGeneratedData.Value)
            {
                return;
            }

            fPed.Task.TurnTo(player.GetUnderlyingPlayer().Character);
            // playerPed.Task.PlayAnimation("anim@gangops@morgue@table@", "player_search");

            var itemList = new GtaStyleText()
                .Blue("Items: ")
                .Reset();
            if (fPed.Items.Count > 0)
            {
                for (var i = 0; i < fPed.Items.Count; i++)
                {
                    if (fPed.Items[i].Illegal)
                    {
                        itemList.Red(fPed.Items[i].Title);
                    }
                    else
                    {
                        itemList.Append(fPed.Items[i].Title);
                    }

                    if (i < fPed.Items.Count - 1)
                    {
                        itemList.Append(", ");
                    }
                }
            }
            else
            {
                itemList.Append("-");
            }

            // playerPed.Task.ClearAnimation("random@domestic", "kneeling_base");
            this._notificationService.ShowNotification(player, itemList.Build());
        };

        items.Add(this.PatDown);

        return items;
    }
}