using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
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
    private readonly IConfigService _configService;
    private readonly IInternalInteractionService _interactionService;

    public InteractionMenu(
        INotificationService notificationService,
        IEntityService entityService,
        ICfxService cfxService,
        IConfigService configService,
        IInteractionService interactionService)
        : base(Controls.InteractionMenu.Id, Controls.InteractionMenu.Title, Controls.InteractionMenu.Key)
    {
        this._notificationService = notificationService;
        this._entityService = entityService;
        this._cfxService = cfxService;
        this._configService = configService;
        this._interactionService = (IInternalInteractionService)interactionService;
    }

    internal MenuButtonItem SearchPed { get; private set; }

    internal MenuListItem AskForLicense { get; private set; }

    internal MenuButtonItem DrugSwabTest { get; private set; }

    internal MenuButtonItem BreathalyzerTest { get; private set; }

    internal MenuButtonItem DetainOrRelease { get; private set; }

    internal MenuButtonItem LetGo { get; private set; }

    internal MenuButtonItem StepOutOfVehicle { get; private set; }

    internal MenuListItem Movement { get; private set; }

    protected override List<MenuItem> CreateItems()
    {
        var items = new List<MenuItem>();

        this.AskForLicense = new MenuListItem("Ask for license", "", new List<MenuListOption>
        {
            new("Driving"),
            new("Fishing"),
            new("Hunting"),
            new("Ccw"),
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
                .White(fPed.Firstname + " " + fPed.Lastname)
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

        this.DrugSwabTest = new MenuButtonItem("Drug swab test", "");
        this.DrugSwabTest.OnSelect += async player =>
        {
            var playerPed = FPed.FromFPlayer(player);

            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || !fPed.HasGeneratedData.Value || fPed.Properties.IsDead)
            {
                return;
            }

            fPed.Task.TurnTo(player.GetUnderlyingPlayer().Character);

            var text = new GtaStyleText()
                .Blue("Drug swab test")
                .NewLine()
                .White(fPed.Drugs.Count == 0 ? "Result: -" : $"Result: {string.Join(",", fPed.Drugs.Select(x => x.Name))}");
            this._notificationService.ShowNotification(player, text.Build());
        };
        items.Add(this.DrugSwabTest);

        this.BreathalyzerTest = new MenuButtonItem("Breathalyzer test", "");
        this.BreathalyzerTest.OnSelect += async player =>
        {
            var playerPed = FPed.FromFPlayer(player);

            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || !fPed.HasGeneratedData.Value || fPed.Properties.IsDead)
            {
                return;
            }

            fPed.Task.TurnTo(player.GetUnderlyingPlayer().Character);

            var text = new GtaStyleText()
                .Blue("Breathalyzer test")
                .NewLine()
                .White($"Legal limit: {this._configService.GeneralConfig.Peds.Alcohol.LegalLimit:0.00}")
                .NewLine()
                .White("Result: ");
            if (fPed.AlcoholOverLegalLimit())
            {
                text.Red(fPed.AlcoholLevel.ToString("0.00"));
            }
            else
            {
                text.White(fPed.AlcoholLevel.ToString("0.00"));
            }

            this._notificationService.ShowNotification(player, text.Build());
        };
        items.Add(this.BreathalyzerTest);

        this.Movement = new MenuListItem("Movement", "", new List<MenuListOption>
        {
            new("Stand still"),
            new("Grab"),
            new("Follow me"),
        });
        this.Movement.OnSelect += async (player, index) =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || !fPed.Stopped.Value || fPed.Properties.IsDead || fPed.Properties.IsInVehicle)
            {
                return;
            }

            var playerPed = FPed.FromFPlayer(player);

            switch (index)
            {
                case 0:
                {
                    if (fPed.Movement.Value == FPedMovement.Grab)
                    {
                        this._cfxService.DetachEntity(fPed.NetworkId, true, false);
                        playerPed.Task.ClearAllImmediately();
                        playerPed.Task.ClearAnimation("doors@", "door_sweep_r_hand_medium");
                    }
                    else if (fPed.Movement.Value == FPedMovement.Follow)
                    {
                        this._interactionService.TogglePedFollowPlayer(player, fPed.GetUnderlyingPed, false);
                    }

                    fPed.Movement.Value = FPedMovement.None;
                    fPed.Task.StandStill(-1);
                    break;
                }
                case 1:
                {
                    fPed.Movement.Value = FPedMovement.Grab;
                    fPed.GetUnderlyingPed.Heading = player.GetUnderlyingPlayer().Character.Heading;
                    playerPed.Task.PlayAnimation(
                        "doors@",
                        "door_sweep_r_hand_medium",
                        5f,
                        -1,
                        FAnimationFlags.StayInEndFrame | FAnimationFlags.UpperBodyOnly | FAnimationFlags.AllowRotation);
                    this._cfxService.AttachEntityToEntity(
                        fPed.NetworkId,
                        player.GetUnderlyingPlayer().Character.NetworkId,
                        0,
                        0.2f,
                        0.4f,
                        0f,
                        0f,
                        0f,
                        0f,
                        false,
                        false,
                        false,
                        false,
                        2,
                        false);
                    break;
                }
                case 2:
                {
                    fPed.Movement.Value = FPedMovement.Follow;
                    this._interactionService.TogglePedFollowPlayer(player, fPed.GetUnderlyingPed, true);
                    break;
                }
            }
        };
        items.Add(this.Movement);

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
                this._notificationService.ShowNotification(player,
                    "The ped is being released."); // TODO: add a custom tooltip in the UI? the native is hidden by the menu
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

        this.StepOutOfVehicle = new MenuButtonItem("Step out of vehicle", "");
        this.StepOutOfVehicle.OnSelect += async player =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || fPed.Properties.IsDead || !fPed.Properties.IsInVehicle)
            {
                return;
            }

            fPed.Task.LeaveVehicle();
        };
        items.Add(this.StepOutOfVehicle);

        return items;
    }
}