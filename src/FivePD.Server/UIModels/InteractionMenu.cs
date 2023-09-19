using System.Collections.Generic;
using System.Threading.Tasks;
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
    private readonly ICfxService _cfxService;

    public InteractionMenu(
        INotificationService notificationService,
        IEntityService entityService,
        ICfxService cfxService)
        : base(Controls.InteractionMenu.Id, Controls.InteractionMenu.Title, Controls.InteractionMenu.Key)
    {
        this._notificationService = notificationService;
        this._entityService = entityService;
        this._cfxService = cfxService;
    }

    internal MenuButtonItem SearchPed { get; private set; }

    internal MenuListItem AskForLicense { get; private set; }

    internal MenuButtonItem DetainOrRelease { get; private set; }

    internal MenuButtonItem LetGo { get; private set; }

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
            if (fPed is null || fPed.Properties.IsDead || !fPed.HasGeneratedData.Value)
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

        this.SearchPed = new MenuButtonItem("Search ped", "");
        this.SearchPed.OnSelect += async player =>
        {
            var playerPed = FPed.FromFPlayer(player);

            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || !fPed.HasGeneratedData.Value || fPed.Properties.IsInVehicle)
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
        items.Add(this.SearchPed);

        this.DetainOrRelease = new MenuButtonItem("Detain/Release the ped", "");
        this.DetainOrRelease.OnSelect += async player =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || fPed.Properties.IsDead || !fPed.Stopped.Value || !fPed.HasGeneratedData.Value || fPed.Properties.IsInVehicle)
            {
                return;
            }

            fPed.Task.AchieveHeading(player.GetUnderlyingPlayer().Character.Heading);
            await Task.Delay(1000);
            fPed.Task.StandStill(-1);
            if (fPed.Detained.Value)
            {
                fPed.Detained.Value = false;
                this._notificationService.ShowNotification(player, "The ped is being released."); // TODO: add a custom tooltip in the UI? the native is hidden by the menu
                this._cfxService.SetEnableHandcuffs(fPed.GetUnderlyingPed.NetworkId, false);
            }
            else
            {
                fPed.Detained.Value = true;
                this._notificationService.ShowNotification(player, "The ped is being detained.");
                this._cfxService.SetEnableHandcuffs(fPed.GetUnderlyingPed.NetworkId, true);
            }
        };
        items.Add(this.DetainOrRelease);

        this.LetGo = new MenuButtonItem("Let go of the ped", "");
        this.LetGo.OnSelect += async player =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || fPed.Properties.IsDead || fPed.Detained.Value)
            {
                return;
            }
            
            this._notificationService.ShowNotification(player, "The ped has been let go.");
            
            fPed.Stopped.Value = false;
            Cfx.API.SetEntityDistanceCullingRadius(fPed.GetUnderlyingPed.Handle, 500f);
            fPed.Properties.SetAlwaysKeepTask(false);
            fPed.Properties.SetBlockPermanentEvents(false);
            fPed.Task.ClearAllImmediately();
            fPed.Properties.RemoveBlips();
            if (fPed.Properties.IsInVehicle)
            {
                // TODO: test
                var vehicleHandle = Cfx.API.GetVehiclePedIsIn(fPed.GetUnderlyingPed.Handle, false);
                var vehicleNetId = Cfx.API.NetworkGetNetworkIdFromEntity(vehicleHandle);
                fPed.Task.CruiseWithVehicle(new Vehicle(vehicleNetId), 15f);
            }
            else
            {
                fPed.Task.WanderAround();
            }
        };
        items.Add(this.LetGo);

        return items;
    }
}