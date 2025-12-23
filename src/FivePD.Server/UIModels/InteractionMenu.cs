using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.Common;
using FivePD.Common.Enums;
using FivePD.Common.Models;
using FivePD.Common.Utilities;
using FivePD.Server.EntityWrappers;
using FivePD.Server.EntityWrappers.PedWrapper;
using FivePD.Server.EntityWrappers.PedWrapper.States;
using FivePD.Server.EntityWrappers.VehicleWrapper;
using FivePD.Server.Interfaces;
using FivePD.Server.Models;
using FivePD.Server.UIModels.MenuModels;
using Microsoft.Extensions.Primitives;
using Cfx = CitizenFX.Core.Native;

namespace FivePD.Server.UIModels;

#pragma warning disable

public sealed class InteractionMenu : Menu
{
    private readonly IGameNotificationService _gameNotificationService;
    private readonly IEntityService _entityService;
    private readonly ICfxService _cfxService;
    private readonly IConfigService _configService;
    private readonly IInternalInteractionService _interactionService;
    private readonly IPedStorageService _pedStorageService;
    private readonly IPedBehaviorService _pedBehaviorService;

    public InteractionMenu(
        IGameNotificationService gameNotificationService,
        IEntityService entityService,
        ICfxService cfxService,
        IConfigService configService,
        IInteractionService interactionService,
        IPedStorageService pedStorageService,
        IPedBehaviorService pedBehaviorService)
        : base(Controls.InteractionMenu.Id, Controls.InteractionMenu.Title, Controls.InteractionMenu.Key)
    {
        this._gameNotificationService = gameNotificationService;
        this._entityService = entityService;
        this._cfxService = cfxService;
        this._configService = configService;
        this._interactionService = (IInternalInteractionService)interactionService;
        this._pedStorageService = pedStorageService;
        this._pedBehaviorService = pedBehaviorService;
    }

    internal MenuButtonItem SearchPed { get; private set; }

    internal MenuListItem AskForLicense { get; private set; }

    internal MenuButtonItem DrugSwabTest { get; private set; }

    internal MenuButtonItem BreathalyzerTest { get; private set; }

    internal MenuButtonItem DetainOrRelease { get; private set; }

    internal MenuButtonItem LetGo { get; private set; }

    internal MenuButtonItem StepOutOfVehicle { get; private set; }

    internal MenuListItem Movement { get; private set; }

    internal MenuButtonItem EnterVehicleAsPassenger { get; private set; }

    public override bool CanOpen(FPlayer player)
    {
        var config = this._configService.GeneralConfig.Menu.Interaction;
        if (config.MustBeOnDuty && !player.IsOnDuty)
        {
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "ERROR.USER.NOT_ON_DUTY"
                });

            return false;
        }

        if (config.MustBeInDepartment && !player.IsInDepartment)
        {
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "ERROR.USER.NOT_IN_DEPARTMENT"
                });
            
            return false;
        }
        
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

            var license = pedData.GetLicense(licenseType);
            if (license is null)
            {
                this._gameNotificationService.Notify(
                    player,
                    new GameNotificationOptions
                    {
                        Title = "FivePD",
                        Message = $"No {licenseTypeLabel} available.",
                    });
                return;
            }

            var text = new StringBuilder()
                .Append($"[{licenseTypeLabel}]")
                .Append("{{NEW_LINE}}")
                .Append(pedData.Firstname + " " + pedData.Lastname)
                .Append("{{NEW_LINE}}")
                .Append($"Issue date: {license.IssueDate:MM-dd-yyyy}")
                .Append("{{NEW_LINE}}")
                .Append($"Expiration date: {license.ExpirationDate:MM-dd-yyyy}")
                .Append("{{NEW_LINE}}")
                .Append("Status: ");

            if (license.Status == PedLicenseStatus.Valid)
            {
                text.Append("Valid");
            }
            else
            {
                text.Append(license.Status switch
                {
                    PedLicenseStatus.Expired => "Expired",
                    PedLicenseStatus.Suspended => "Suspended",
                    PedLicenseStatus.Revoked => "Revoked",
                    _ => "Unknown",
                });
            }

            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = text.ToString(),
                });
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

            var itemList = new StringBuilder().Append("Items: ");
            if (pedData.Items.Count > 0)
            {
                for (var i = 0; i < pedData.Items.Count; i++)
                {
                    if (pedData.Items[i].Illegal)
                    {
                        itemList.Append(pedData.Items[i].Title);
                    }
                    else
                    {
                        itemList.Append(pedData.Items[i].Title);
                    }

                    if (i < pedData.Items.Count - 1)
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
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = itemList.ToString(),
                });
        };
        items.Add(this.SearchPed);

        this.DrugSwabTest = new MenuButtonItem("Drug swab test", "");
        this.DrugSwabTest.OnSelect += async player =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || !fPed.HasGeneratedData.Value || fPed.Properties.IsDead)
            {
                return;
            }

            if (!fPed.Properties.IsInVehicle)
            {
                fPed.Task.TurnTo(player.GetUnderlyingPlayer().Character);
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

            var text = new StringBuilder()
                .Append("Drug swab test")
                .Append("{{NEW_LINE}}")
                .Append(pedData.Drugs.Count == 0 ? "Result: -" : $"Result: {string.Join(", ", pedData.Drugs.Select(x => x.Name))}");
            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = text.ToString(),
                });
        };
        items.Add(this.DrugSwabTest);

        this.BreathalyzerTest = new MenuButtonItem("Breathalyzer test", "");
        this.BreathalyzerTest.OnSelect += async player =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || !fPed.HasGeneratedData.Value || fPed.Properties.IsDead)
            {
                return;
            }

            if (!fPed.Properties.IsInVehicle)
            {
                fPed.Task.TurnTo(player.GetUnderlyingPlayer().Character);
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

            var text = new StringBuilder()
                .Append("Breathalyzer test")
                .Append("{{NEW_LINE}}")
                .Append($"Legal limit: {this._configService.GeneralConfig.Peds.Alcohol.LegalLimit:0.00}")
                .Append("{{NEW_LINE}}")
                .Append("Result: ");
            if (pedData.AlcoholOverLegalLimit())
            {
                text.Append(pedData.AlcoholLevel.ToString("0.00"));
            }
            else
            {
                text.Append(pedData.AlcoholLevel.ToString("0.00"));
            }

            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = text.ToString(),
                });
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

            this._pedBehaviorService.StopBehavior(fPed);
            var playerPed = FPed.FromFPlayer(player);

            switch (index)
            {
                case 0:
                {
                    fPed.ChangeState(new FPedStandStillState(fPed));
                    break;
                }
                case 1:
                {
                    fPed.ChangeState(new FPedGrabbedState(fPed, playerPed));
                    break;
                }
                case 2:
                {
                    fPed.ChangeState(new FPedFollowPlayerState(fPed, player));
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

            fPed.ChangeState(new FPedTurnToHeadingThenStandStillState(fPed, player.GetUnderlyingPlayer().Character.Heading));

            if (fPed.Detained.Value)
            {
                fPed.Detained.Value = false;
                this._gameNotificationService.Notify(
                    player,
                    new GameNotificationOptions
                    {
                        Title = "FivePD",
                        Message = "The ped is being released.",
                    });
                this._cfxService.SetEnableHandcuffs(fPed.GetUnderlyingPed.NetworkId, false);
            }
            else
            {
                fPed.Detained.Value = true;
                this._gameNotificationService.Notify(
                    player,
                    new GameNotificationOptions
                    {
                        Title = "FivePD",
                        Message = "The ped is being detained.",
                    });
                this._cfxService.SetEnableHandcuffs(fPed.GetUnderlyingPed.NetworkId, true);
                this._pedBehaviorService.StopBehavior(fPed);
            }
        };
        items.Add(this.DetainOrRelease);

        this.LetGo = new MenuButtonItem("Let go of the ped", "");
        this.LetGo.OnSelect += async player =>
        {
            var fPed = await this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            if (fPed is null || fPed.Properties.IsDead)
            {
                return;
            }

            if (fPed.Detained.Value)
            {
                this._gameNotificationService.Notify(
                    player,
                    new GameNotificationOptions
                    {
                        Title = "FivePD",
                        Message = "You cannot let go of a ped that is detained.",
                    });
                return;
            }

            this._gameNotificationService.Notify(
                player,
                new GameNotificationOptions
                {
                    Title = "FivePD",
                    Message = "The ped has been let go.",
                });

            this._pedBehaviorService.StopBehavior(fPed);
            fPed.Stopped.Value = false;
            Cfx.API.SetEntityDistanceCullingRadius(fPed.GetUnderlyingPed.Handle, 500f);
            fPed.Properties.SetAlwaysKeepTask(false);
            fPed.Properties.SetBlockPermanentEvents(false);
            this._pedBehaviorService.MakeNonCompliant(fPed);
            fPed.Properties.RemoveBlips();
            if (fPed.Properties.IsInVehicle)
            {
                var vehicleHandle = Cfx.API.GetVehiclePedIsIn(fPed.GetUnderlyingPed.Handle, false);
                var vehicleNetId = Cfx.API.NetworkGetNetworkIdFromEntity(vehicleHandle);
                fPed.ChangeState(new FPedCruiseWithVehicleState(fPed, vehicleNetId));
            }
            else
            {
                fPed.ChangeState(new FPedWanderAroundState(fPed));
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

            this._pedBehaviorService.StopBehavior(fPed);
            fPed.ChangeState(new FPedStepOutVehicleThenStandStillState(fPed));
        };
        items.Add(this.StepOutOfVehicle);

        this.EnterVehicleAsPassenger = new MenuButtonItem("Order ped to enter vehicle", "");
        this.EnterVehicleAsPassenger.OnSelect += async player =>
        {
            var pedTask = this._entityService.GetClosestEntityToPlayerAsync<FPed>(player.GetUnderlyingPlayer());
            var vehicleTask = this._entityService.GetClosestEntityToPlayerAsync<FVehicle>(player.GetUnderlyingPlayer());

            await Task.WhenAll(pedTask, vehicleTask);

            var fPed = await pedTask;
            var fVehicle = await vehicleTask;
            if (fPed is null || fPed.Properties.IsDead || fPed.Properties.IsInVehicle || fVehicle is null)
            {
                return;
            }

            this._pedBehaviorService.StopBehavior(fPed);
            this._pedBehaviorService.MakeCompliant(fPed);
            fPed.ChangeState(new FPedEnterVehicleAsPassengerState(fPed, fVehicle));
        };
        items.Add(this.EnterVehicleAsPassenger);

        return items;
    }
}